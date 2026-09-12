from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.rest_routes import router as envios_router
from app.soap_service import router as soap_router

app = FastAPI(
    title="Microservicio de Envíos y Cálculo de Rutas (Python)",
    description="Módulo logístico para registro y rastreo de envíos (REST) y cálculo de tarifas de paquetería (SOAP). Práctica de Programación en Ambiente Cliente-Servidor.",
    version="1.0.0",
    docs_url="/docs",
    redoc_url="/redoc",
    openapi_url="/openapi.json"
)

# Configuración CORS global
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Inclusión de rutas REST
app.include_router(envios_router)

# Inclusión de rutas SOAP (POST /soap/envios y GET /soap/envios?wsdl)
app.include_router(soap_router)

@app.get("/", summary="Health Check")
def health_check():
    return {
        "status": "healthy",
        "service": "Envios y Rutas (Python)",
        "rest_docs": "/docs",
        "soap_wsdl": "/soap/envios?wsdl",
        "port": 8082
    }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("app.main:app", host="0.0.0.0", port=8082, reload=True)
