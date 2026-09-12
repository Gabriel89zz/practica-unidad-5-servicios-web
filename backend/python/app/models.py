from pydantic import BaseModel, Field
from typing import List, Optional
from datetime import datetime

class EventoRastreo(BaseModel):
    timestamp: str
    estado: str
    ubicacion: str
    descripcion: str

class CrearEnvioRequest(BaseModel):
    remitente_nombre: str = Field(..., example="TechStore CDMX")
    remitente_direccion: str = Field(..., example="Av. Insurgentes Sur 1602, Benito Juárez, CDMX")
    destinatario_nombre: str = Field(..., example="Juan Pérez López")
    destinatario_direccion: str = Field(..., example="Calle Hidalgo 45, Guadalajara, Jalisco")
    peso_kg: float = Field(..., gt=0, example=2.5)
    tipo_servicio: str = Field(default="ESTANDAR", example="EXPRESS") # ESTANDAR, EXPRESS, ECONOMICO
    descripcion_contenido: Optional[str] = Field(default="Artículos de cómputo", example="Laptop y accesorios")

class ActualizarEstadoRequest(BaseModel):
    nuevo_estado: str = Field(..., example="EN_TRANSITO") # PREPARACION, EN_TRANSITO, EN_REPARTO, ENTREGADO, DEVUELTO
    ubicacion_actual: str = Field(..., example="Centro de Distribución Guadalajara")
    comentario: Optional[str] = Field(default="Paquete recibido en hub logístico")

class EnvioResponse(BaseModel):
    numero_guia: str
    remitente_nombre: str
    remitente_direccion: str
    destinatario_nombre: str
    destinatario_direccion: str
    peso_kg: float
    tipo_servicio: str
    costo_envio: float
    estado: str
    fecha_creacion: str
    fecha_entrega_estimada: str
    historial_eventos: List[EventoRastreo]
