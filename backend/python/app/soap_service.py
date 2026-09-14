import base64
import xml.etree.ElementTree as ET
from pathlib import Path
from fastapi import APIRouter, Request, Response
from app.storage import envio_storage

router = APIRouter(prefix="/soap/envios", tags=["SOAP Envíos y Logística"])

SOAP_USER = "admin"
SOAP_PASS = "admin_pass_2026"
NAMESPACE_URI = "http://ecommerce.com/envios"

# Carga del contrato WSDL formal desde archivo externo (Best Practice)
WSDL_PATH = Path(__file__).parent / "envios.wsdl"

def get_wsdl_content() -> str:
    """Carga el contrato WSDL formal desde el archivo local envios.wsdl."""
    if WSDL_PATH.exists():
        return WSDL_PATH.read_text(encoding="utf-8")
    return "<!-- Error: Contrato WSDL no encontrado en el servidor -->"

def generate_soap_fault(fault_code: str, fault_string: str) -> str:
    """Genera una estructura estándar SOAP Fault (HTTP 500) ante excepciones."""
    return f"""<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
  <soapenv:Body>
    <soapenv:Fault>
      <faultcode>{fault_code}</faultcode>
      <faultstring>{fault_string}</faultstring>
      <detail>
        <errorCode>SOAP_ERROR</errorCode>
      </detail>
    </soapenv:Fault>
  </soapenv:Body>
</soapenv:Envelope>"""

@router.get("", summary="WSDL del Servicio SOAP de Envíos")
def get_wsdl(request: Request):
    """
    Retorna el contrato WSDL formal del servicio SOAP de envíos.
    Accesible en /soap/envios?wsdl o GET directo.
    """
    wsdl_xml = get_wsdl_content()
    return Response(content=wsdl_xml, media_type="text/xml; charset=utf-8")

@router.post("", summary="Dispatcher SOAP de Envíos")
async def handle_soap(request: Request):
    """
    Dispatcher principal para el procesamiento de peticiones SOAP XML.
    Valida credenciales, parsea el envoltorio XML e invoca la operación correspondiente.
    """
    raw_body = await request.body()
    xml_str = raw_body.decode("utf-8", errors="ignore")

    # 1. Validación de autenticación (Basic Auth o SOAP Header)
    auth_header = request.headers.get("authorization", "")
    if auth_header.startswith("Basic "):
        try:
            decoded = base64.b64decode(auth_header[6:]).decode("utf-8")
            user, password = decoded.split(":", 1)
            if user != SOAP_USER or password != SOAP_PASS:
                fault = generate_soap_fault("soapenv:Client.AuthenticationFailed", "Credenciales SOAP inválidas.")
                return Response(content=fault, status_code=500, media_type="text/xml; charset=utf-8")
        except Exception:
            fault = generate_soap_fault("soapenv:Client.AuthenticationFailed", "Fallo al decodificar credenciales Basic Auth.")
            return Response(content=fault, status_code=500, media_type="text/xml; charset=utf-8")

    # 2. Parsear el XML del sobre SOAP
    try:
        root = ET.fromstring(xml_str)
    except Exception as e:
        fault = generate_soap_fault("soapenv:Client.XMLParseError", f"Error de sintaxis en el sobre SOAP: {str(e)}")
        return Response(content=fault, status_code=500, media_type="text/xml; charset=utf-8")

    # Remover namespaces para búsqueda simplificada de elementos
    for elem in root.iter():
        if '}' in elem.tag:
            elem.tag = elem.tag.split('}', 1)[1]

    # Identificar la operación solicitada
    if root.find(".//CalcularTarifa") is not None or root.find(".//CalcularCostoEnvio") is not None:
        return handle_calcular_tarifa(root)
    elif root.find(".//ConsultarGuia") is not None or root.find(".//RastrearEnvio") is not None:
        return handle_consultar_guia(root)
    else:
        fault = generate_soap_fault("soapenv:Client.UnknownOperation", "Operación no reconocida en el cuerpo SOAP.")
        return Response(content=fault, status_code=500, media_type="text/xml; charset=utf-8")

def handle_calcular_tarifa(root: ET.Element) -> Response:
    """Procesa la operación SOAP CalcularTarifa."""
    elem_origen = root.find(".//origen")
    elem_destino = root.find(".//destino")
    elem_peso = root.find(".//peso_kg")
    elem_servicio = root.find(".//tipo_servicio")

    origen = elem_origen.text.strip() if elem_origen is not None and elem_origen.text else "CDMX"
    destino = elem_destino.text.strip() if elem_destino is not None and elem_destino.text else "Destino"
    try:
        peso = float(elem_peso.text.strip()) if elem_peso is not None and elem_peso.text else 1.0
    except ValueError:
        peso = 1.0
    tipo_servicio = elem_servicio.text.strip().upper() if elem_servicio is not None and elem_servicio.text else "ESTANDAR"

    distancia = 450
    if "guadalajara" in destino.lower(): distancia = 550
    elif "monterrey" in destino.lower(): distancia = 900
    elif "merida" in destino.lower(): distancia = 1300

    factor = 1.8 if tipo_servicio == "EXPRESS" else (0.8 if tipo_servicio == "ECONOMICO" else 1.0)
    dias = 1 if tipo_servicio == "EXPRESS" else (5 if tipo_servicio == "ECONOMICO" else 3)
    costo_base = 100.0 + (peso * 35.0) + (distancia * 0.15)
    costo_final = round(costo_base * factor, 2)

    response_xml = f"""<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tns="http://ecommerce.com/envios">
  <soapenv:Body>
    <tns:CalcularTarifaResponse>
      <tns:CalcularTarifaResult>
        <tns:origen>{origen}</tns:origen>
        <tns:destino>{destino}</tns:destino>
        <tns:peso_kg>{peso}</tns:peso_kg>
        <tns:tipo_servicio>{tipo_servicio}</tns:tipo_servicio>
        <tns:costo_envio>{costo_final}</tns:costo_envio>
        <tns:tiempo_estimado_dias>{dias}</tns:tiempo_estimado_dias>
        <tns:moneda>MXN</tns:moneda>
        <tns:distancia_aprox_km>{distancia}</tns:distancia_aprox_km>
      </tns:CalcularTarifaResult>
    </tns:CalcularTarifaResponse>
  </soapenv:Body>
</soapenv:Envelope>"""
    return Response(content=response_xml, status_code=200, media_type="text/xml; charset=utf-8")

def handle_consultar_guia(root: ET.Element) -> Response:
    """Procesa la operación SOAP ConsultarGuia."""
    elem_guia = root.find(".//numero_guia")
    guia = elem_guia.text.strip() if elem_guia is not None and elem_guia.text else ""

    envio = envio_storage.obtener_por_guia(guia)
    if envio:
        estado = envio["estado"]
        fecha = envio["fecha_entrega_estimada"]
        ult = envio["historial_eventos"][-1] if envio["historial_eventos"] else {}
        ubicacion = ult.get("ubicacion", "En ruta")
        encontrado = "true"
    else:
        estado = "NO_ENCONTRADO"
        fecha = "N/A"
        ubicacion = "Desconocida"
        encontrado = "false"

    response_xml = f"""<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tns="http://ecommerce.com/envios">
  <soapenv:Body>
    <tns:ConsultarGuiaResponse>
      <tns:ConsultarGuiaResult>
        <tns:numero_guia>{guia}</tns:numero_guia>
        <tns:estado>{estado}</tns:estado>
        <tns:ubicacion_actual>{ubicacion}</tns:ubicacion_actual>
        <tns:fecha_estimada>{fecha}</tns:fecha_estimada>
        <tns:encontrado>{encontrado}</tns:encontrado>
      </tns:ConsultarGuiaResult>
    </tns:ConsultarGuiaResponse>
  </soapenv:Body>
</soapenv:Envelope>"""
    return Response(content=response_xml, status_code=200, media_type="text/xml; charset=utf-8")
