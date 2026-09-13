#!/usr/bin/env python3
"""
Suite de Pruebas Automatizadas End-to-End
Sistema Distribuido de Comercio Electrónico y Logística (Práctica Cliente-Servidor)
Prueba los 6 microservicios en REST y SOAP.
"""

import sys
import json
import os
import urllib.request
import urllib.error
import xml.etree.ElementTree as ET

HOST = "http://localhost"
if len(sys.argv) > 1:
    if sys.argv[1].startswith("--host="):
        HOST = sys.argv[1].split("=")[1].rstrip("/")
    elif sys.argv[1] == "--host" and len(sys.argv) > 2:
        HOST = sys.argv[2].rstrip("/")
    elif not sys.argv[1].startswith("-"):
        HOST = sys.argv[1].rstrip("/")
elif "SERVICES_HOST" in os.environ:
    HOST = os.environ["SERVICES_HOST"].rstrip("/")

BASE_URLS = {
    "java": f"{HOST}:8081",
    "python": f"{HOST}:8082",
    "php": f"{HOST}:8083",
    "ruby": f"{HOST}:8084",
    "csharp": f"{HOST}:8085",
    "vbnet": f"{HOST}:8086"
}

VALID_AUTH_HEADERS = {
    "Authorization": "Bearer test_token_2026",
    "X-API-Key": "secret_key_cs"
}

def make_http_request(url, method="GET", headers=None, body=None):
    hdrs = headers or {}
    data = body.encode("utf-8") if isinstance(body, str) else body
    req = urllib.request.Request(url, data=data, headers=hdrs, method=method)
    try:
        with urllib.request.urlopen(req, timeout=10) as resp:
            content = resp.read().decode("utf-8")
            return resp.status, content, resp.headers
    except urllib.error.HTTPError as e:
        content = e.read().decode("utf-8")
        return e.code, content, e.headers
    except Exception as e:
        return None, str(e), {}

def test_service_health(name, base_url):
    print(f"\n[+] Verificando disponibilidad de {name.upper()} ({base_url})...")
    code, content, _ = make_http_request(base_url + "/")
    if code in (200, 404):
        print(f"  [OK] Servicio responde (HTTP {code})")
        return True
    else:
        print(f"  [AVISO] No se pudo conectar a {base_url} ({content})")
        return False

def test_rest_security(name, api_path):
    print(f"  [*] Probando filtro de seguridad REST en {api_path}...")
    # 1. Petición sin token -> Debe ser 401
    code, content, _ = make_http_request(api_path, method="GET")
    if code == 401:
        print("    [PASS] 401 Unauthorized sin credenciales verificado.")
    else:
        print(f"    [FAIL] Se esperaba 401, se obtuvo {code}")

    # 2. Petición con token válido -> Debe ser 200
    headers = {"Authorization": "Bearer test_token_2026"}
    code, content, _ = make_http_request(api_path, method="GET", headers=headers)
    if code == 200:
        print("    [PASS] 200 OK con Bearer Token verificado.")
    else:
        print(f"    [FAIL] Se esperaba 200, se obtuvo {code}: {content[:100]}")

def test_soap_wsdl(name, wsdl_url):
    print(f"  [*] Verificando contrato WSDL en {wsdl_url}...")
    code, content, _ = make_http_request(wsdl_url, method="GET")
    if code == 200 and ("<wsdl:definitions" in content or "<definitions" in content):
        print("    [PASS] Contrato WSDL recuperado y válido.")
    else:
        print(f"    [FAIL] Error al recuperar WSDL (HTTP {code})")

def test_soap_invocation(name, url, xml_payload, action_header=""):
    print(f"  [*] Invocando operación SOAP en {url}...")
    headers = {
        "Content-Type": "text/xml; charset=utf-8",
        "SOAPAction": action_header
    }
    code, content, _ = make_http_request(url, method="POST", headers=headers, body=xml_payload)
    if code == 200 and ("envelope" in content.lower() and "body" in content.lower()):
        print("    [PASS] Operación SOAP ejecutada con éxito (HTTP 200 + Envelope XML).")
    else:
        print(f"    [FAIL] Fallo en invocación SOAP (HTTP {code}): {content[:150]}")

def main():
    print("=" * 70)
    print("SUITE DE PRUEBAS DE INTEGRACIÓN: REST Y SOAP (ECOSISTEMA DISTRIBUIDO)")
    print("=" * 70)

    available_count = 0
    for name, base_url in BASE_URLS.items():
        if test_service_health(name, base_url):
            available_count += 1

    if available_count == 0:
        print("\n[!] Ningún servicio está activo en los puertos 8081-8086.")
        print("    Para ejecutar los servicios:")
        print("    1. Con Docker: docker-compose up --build")
        print("    2. Localmente: ejecute los microservicios en terminales independientes.")
        return

    # 1. Java (Catálogo e Inventario)
    if test_service_health("java", BASE_URLS["java"]):
        test_rest_security("java", BASE_URLS["java"] + "/api/v1/productos")
        test_soap_wsdl("java", BASE_URLS["java"] + "/ws/inventario.wsdl")
        xml_java = """<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:inv="http://ecommerce.com/inventario"><soapenv:Body><inv:ConsultarStockRequest><inv:sku>LAP-001</inv:sku></inv:ConsultarStockRequest></soapenv:Body></soapenv:Envelope>"""
        test_soap_invocation("java", BASE_URLS["java"] + "/ws", xml_java)

    # 2. Python (Envíos)
    if test_service_health("python", BASE_URLS["python"]):
        test_rest_security("python", BASE_URLS["python"] + "/api/v1/envios")
        test_soap_wsdl("python", BASE_URLS["python"] + "/soap/envios?wsdl")
        xml_py = """<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:env="http://ecommerce.com/envios"><soapenv:Body><env:CalcularTarifa><env:origen>CDMX</env:origen><env:destino>Guadalajara</env:destino><env:peso_kg>3.0</env:peso_kg><env:tipo_servicio>EXPRESS</env:tipo_servicio></env:CalcularTarifa></soapenv:Body></soapenv:Envelope>"""
        test_soap_invocation("python", BASE_URLS["python"] + "/soap/envios", xml_py)

    # 3. PHP (Órdenes)
    if test_service_health("php", BASE_URLS["php"]):
        test_rest_security("php", BASE_URLS["php"] + "/api/v1/ordenes")
        test_soap_wsdl("php", BASE_URLS["php"] + "/soap/ordenes.wsdl")
        xml_php = """<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ord="http://ecommerce.com/ordenes"><soapenv:Body><ord:ValidarEstadoOrden><ordenId>ORD-2026-101</ordenId></ord:ValidarEstadoOrden></soapenv:Body></soapenv:Envelope>"""
        test_soap_invocation("php", BASE_URLS["php"] + "/soap/ordenes", xml_php)

    # 4. Ruby (Notificaciones)
    if test_service_health("ruby", BASE_URLS["ruby"]):
        test_rest_security("ruby", BASE_URLS["ruby"] + "/api/v1/destinatarios")
        test_soap_wsdl("ruby", BASE_URLS["ruby"] + "/soap/notificaciones/wsdl")
        xml_ruby = """<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:not="http://ecommerce.com/notificaciones"><soapenv:Body><not:DespacharAlertaCritica><tipoAlerta>PEDIDO_CREADO</tipoAlerta><canal>EMAIL</canal><destinatario>test@example.com</destinatario><mensaje>Orden creada</mensaje><prioridad>ALTA</prioridad></not:DespacharAlertaCritica></soapenv:Body></soapenv:Envelope>"""
        test_soap_invocation("ruby", BASE_URLS["ruby"] + "/soap/notificaciones", xml_ruby)

    # 5. C# (Facturación)
    if test_service_health("csharp", BASE_URLS["csharp"]):
        test_rest_security("csharp", BASE_URLS["csharp"] + "/api/v1/facturas")
        test_soap_wsdl("csharp", BASE_URLS["csharp"] + "/soap/FacturacionService.svc?wsdl")
        xml_cs = """<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><TimbrarComprobante xmlns="http://ecommerce.com/facturacion"><request xmlns:i="http://www.w3.org/2001/XMLSchema-instance"><RfcEmisor>ECO20260101ECO</RfcEmisor><RfcReceptor>GOLF850315ABC</RfcReceptor><Subtotal>1000.00</Subtotal><Iva>160.00</Iva><Total>1160.00</Total><Concepto>Prueba de timbrado SAT</Concepto></request></TimbrarComprobante></s:Body></s:Envelope>"""
        test_soap_invocation("csharp", BASE_URLS["csharp"] + "/soap/FacturacionService.svc", xml_cs, "http://ecommerce.com/facturacion/IFacturacionService/TimbrarComprobante")

    # 6. VB.NET (Auditoría)
    if test_service_health("vbnet", BASE_URLS["vbnet"]):
        test_rest_security("vbnet", BASE_URLS["vbnet"] + "/api/v1/logs")
        test_soap_wsdl("vbnet", BASE_URLS["vbnet"] + "/soap/AuditoriaService.svc?wsdl")
        xml_vb = """<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><RegistrarEvento xmlns="http://ecommerce.com/auditoria"><request xmlns:i="http://www.w3.org/2001/XMLSchema-instance"><ServicioOrigen>TEST_SUITE</ServicioOrigen><Accion>EJECUCION_PRUEBA</Accion><Usuario>test_runner</Usuario><Detalles>Prueba automatizada de integracion</Detalles><Nivel>INFO</Nivel></request></RegistrarEvento></s:Body></s:Envelope>"""
        test_soap_invocation("vbnet", BASE_URLS["vbnet"] + "/soap/AuditoriaService.svc", xml_vb, "http://ecommerce.com/auditoria/IAuditoriaService/RegistrarEvento")

    print("\n" + "=" * 70)
    print("EJECUCIÓN DE PRUEBAS FINALIZADA")
    print("=" * 70)

if __name__ == "__main__":
    main()
