import threading
import uuid
from datetime import datetime, timedelta
from typing import Dict, List, Optional
from app.models import EnvioResponse, EventoRastreo

class EnvioStorage:
    def __init__(self):
        self._lock = threading.Lock()
        self._envios: Dict[str, dict] = {}
        self._seed_data()

    def _seed_data(self):
        guia_demo = "GUIA-2026-9081"
        ahora = datetime.utcnow()
        self._envios[guia_demo] = {
            "numero_guia": guia_demo,
            "remitente_nombre": "Almacén Central Tech",
            "remitente_direccion": "Av. Reforma 222, CDMX",
            "destinatario_nombre": "Carlos Mendoza Ruiz",
            "destinatario_direccion": "Av. Vallarta 3200, Zapopan, Jalisco",
            "peso_kg": 3.2,
            "tipo_servicio": "EXPRESS",
            "costo_envio": 245.50,
            "estado": "EN_TRANSITO",
            "fecha_creacion": (ahora - timedelta(days=1)).isoformat() + "Z",
            "fecha_entrega_estimada": (ahora + timedelta(days=1)).strftime("%Y-%m-%d"),
            "historial_eventos": [
                {
                    "timestamp": (ahora - timedelta(days=1)).isoformat() + "Z",
                    "estado": "PREPARACION",
                    "ubicacion": "Almacén Central CDMX",
                    "descripcion": "Guía generada y paquete empaquetado."
                },
                {
                    "timestamp": (ahora - timedelta(hours=14)).isoformat() + "Z",
                    "estado": "EN_TRANSITO",
                    "ubicacion": "Centro Operativo Querétaro",
                    "descripcion": "En tránsito a destino regional."
                }
            ]
        }

    def listar_todos(self) -> List[dict]:
        with self._lock:
            return list(self._envios.values())

    def obtener_por_guia(self, guia: str) -> Optional[dict]:
        with self._lock:
            return self._envios.get(guia.strip().upper())

    def crear_envio(self, datos: dict) -> dict:
        with self._lock:
            numero_guia = f"GUIA-2026-{uuid.uuid4().hex[:6].upper()}"
            ahora = datetime.utcnow()
            
            # Cálculo de costo aproximado
            peso = float(datos.get("peso_kg", 1.0))
            factor_servicio = 1.8 if datos.get("tipo_servicio") == "EXPRESS" else 1.0
            costo = round((120.0 + (peso * 25.0)) * factor_servicio, 2)
            dias_entrega = 1 if datos.get("tipo_servicio") == "EXPRESS" else 3

            nuevo = {
                "numero_guia": numero_guia,
                "remitente_nombre": datos["remitente_nombre"],
                "remitente_direccion": datos["remitente_direccion"],
                "destinatario_nombre": datos["destinatario_nombre"],
                "destinatario_direccion": datos["destinatario_direccion"],
                "peso_kg": peso,
                "tipo_servicio": datos.get("tipo_servicio", "ESTANDAR"),
                "costo_envio": costo,
                "estado": "PREPARACION",
                "fecha_creacion": ahora.isoformat() + "Z",
                "fecha_entrega_estimada": (ahora + timedelta(days=dias_entrega)).strftime("%Y-%m-%d"),
                "historial_eventos": [
                    {
                        "timestamp": ahora.isoformat() + "Z",
                        "estado": "PREPARACION",
                        "ubicacion": datos.get("remitente_direccion", "Origen"),
                        "descripcion": "Paquete registrado en el sistema logístico."
                    }
                ]
            }
            self._envios[numero_guia] = nuevo
            return nuevo

    def actualizar_estado(self, guia: str, nuevo_estado: str, ubicacion: str, comentario: str) -> Optional[dict]:
        with self._lock:
            envio = self._envios.get(guia.strip().upper())
            if not envio:
                return None
            envio["estado"] = nuevo_estado.upper()
            evento = {
                "timestamp": datetime.utcnow().isoformat() + "Z",
                "estado": nuevo_estado.upper(),
                "ubicacion": ubicacion,
                "descripcion": comentario or f"Cambio de estado a {nuevo_estado}"
            }
            envio["historial_eventos"].append(evento)
            return envio

envio_storage = EnvioStorage()
