from fastapi import APIRouter, Depends, HTTPException, status
from typing import List, Optional
from app.models import CrearEnvioRequest, ActualizarEstadoRequest, EnvioResponse
from app.storage import envio_storage
from app.auth import verify_rest_auth

router = APIRouter(
    prefix="/api/v1/envios",
    tags=["Envíos y Logística"],
    dependencies=[Depends(verify_rest_auth)]
)

@router.get("", response_model=List[EnvioResponse], summary="Listar todos los envíos")
def listar_envios():
    """
    Retorna la lista completa de paquetes y órdenes de envío registrados en la plataforma.
    Requiere autenticación mediante Bearer Token o API-Key.
    """
    return envio_storage.listar_todos()

@router.post("", response_model=EnvioResponse, status_code=status.HTTP_201_CREATED, summary="Registrar nuevo envío")
def crear_envio(datos: CrearEnvioRequest):
    """
    Registra un nuevo envío calculando automáticamente costos logísticos y fecha estimada de entrega.
    """
    nuevo = envio_storage.crear_envio(datos.dict())
    return nuevo

@router.get("/{numero_guia}", response_model=EnvioResponse, summary="Rastrear paquete por número de guía")
def rastrear_envio(numero_guia: str):
    """
    Consulta la bitácora completa y estado actual de una guía logística específica.
    """
    envio = envio_storage.obtener_por_guia(numero_guia)
    if not envio:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail={
                "error": "Not Found",
                "message": f"No se encontró el envío con número de guía '{numero_guia}'",
                "statusCode": 404
            }
        )
    return envio

@router.put("/{numero_guia}/estado", response_model=EnvioResponse, summary="Actualizar estado del envío")
def actualizar_estado(numero_guia: str, datos: ActualizarEstadoRequest):
    """
    Actualiza el estado logístico del paquete (EN_TRANSITO, EN_REPARTO, ENTREGADO) agregando un hito en la bitácora.
    """
    actualizado = envio_storage.actualizar_estado(
        guia=numero_guia,
        nuevo_estado=datos.nuevo_estado,
        ubicacion=datos.ubicacion_actual,
        comentario=datos.comentario
    )
    if not actualizado:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail={
                "error": "Not Found",
                "message": f"No se encontró el envío con número de guía '{numero_guia}'",
                "statusCode": 404
            }
        )
    return actualizado
