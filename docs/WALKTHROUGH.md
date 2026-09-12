# Walkthrough: Ecosistema Distribuido de Comercio Electrónico y Logística

Implementación completa de la arquitectura de servicios web distribuidos heterogéneos para la práctica de **Programación en Ambiente Cliente-Servidor (7.° Semestre de Ingeniería en Informática)**.

---

## 1. Resumen de Entregables Desarrollados

Se diseñó e implementó un monorepo distribuido que integra **6 microservicios independientes** desarrollados en 6 lenguajes y runtimes diferentes (4 de Software Libre y 2 Empresariales de la plataforma .NET), cada uno exponiendo simultáneamente interfaces **RESTful (JSON)** y **SOAP (XML/WSDL)**:

```text
practica-unidad-5/
├── docker-compose.yml             # Orquestación de los 6 microservicios en puertos 8081-8086
├── .gitignore
├── README.md                      # Manual general y guía de despliegue
├── backend/
│   ├── java/                      # 1. Java 21 / Spring Boot 3 (Catálogo e Inventario) [:8081]
│   ├── python/                    # 2. Python 3.12 / FastAPI + Spyne (Envíos y Rutas) [:8082]
│   ├── php/                       # 3. PHP 8.2+ / Slim 4 + SQLite (Gestión de Órdenes) [:8083]
│   ├── ruby/                      # 4. Ruby 3.2+ / Sinatra + Puma (Notificaciones) [:8084]
│   ├── csharp/                    # 5. C# .NET 9 / CoreWCF (Facturación y Timbrado SAT) [:8085]
│   ├── vbnet/                     # 6. VB.NET .NET 9 / CoreWCF (Auditoría y Bitácoras) [:8086]
│   └── shared/                    # Puente de extensiones CoreWCF para VB.NET
└── docs/
    ├── ARCHITECTURE.md            # Justificación de diseño, diagramas y análisis SL vs Propietario
    ├── API_REFERENCE.md           # Catálogo detallado de endpoints REST con ejemplos curl
    ├── SOAP_ENVELOPES.md          # Envoltorios XML de solicitud/respuesta para Postman y SoapUI
    ├── test_suite.py              # Suite de pruebas automatizadas en Python
    ├── test_all.ps1               # Suite de pruebas automatizadas en PowerShell
    ├── index.html                 # Dashboard interactivo y consola web de pruebas en vivo
    └── WALKTHROUGH.md             # Resumen de validaciones y entregables
```

---

## 2. Catálogo de Microservicios Implementados

| # | Módulo | Categoría | Stack Tecnológico | Puerto | Operaciones REST | Operaciones SOAP | Contratos / Docs |
| :-: | :--- | :--- | :--- | :-: | :--- | :--- | :--- |
| **1** | **Catálogo e Inventario** | Software Libre | Java 21 / Spring Boot 3 / Spring-WS | `8081` | `GET/POST/PUT/DELETE /api/v1/productos` | `ConsultarStockRequest`<br/>`ActualizarStockRequest` | WSDL: `/ws/inventario.wsdl`<br/>Swagger: `/swagger-ui.html` |
| **2** | **Envíos y Rutas** | Software Libre | Python 3.12 / FastAPI / Spyne | `8082` | `GET/POST /api/v1/envios`<br/>`PUT /api/v1/envios/{guia}/estado` | `CalcularTarifa`<br/>`ConsultarGuia` | WSDL: `/soap/envios?wsdl`<br/>Swagger: `/docs` |
| **3** | **Gestión de Órdenes** | Software Libre | PHP 8.2 / Slim 4 / SQLite PDO | `8083` | `GET/POST /api/v1/ordenes`<br/>`DELETE /api/v1/ordenes/{id}` | `ValidarEstadoOrden` | WSDL: `/soap/ordenes.wsdl`<br/>Info: `/` |
| **4** | **Notificaciones & Webhooks** | Software Libre | Ruby 3.2 / Sinatra / Puma | `8084` | `GET/POST /api/v1/destinatarios`<br/>`GET/POST /api/v1/plantillas` | `DespacharAlertaCritica` | WSDL: `/soap/notificaciones/wsdl`<br/>Info: `/` |
| **5** | **Facturación y Timbrado** | Propietario / .NET | C# .NET 9 / CoreWCF | `8085` | `GET/POST /api/v1/facturas`<br/>`GET /api/v1/facturas/comprobante/{uuid}` | `TimbrarComprobante`<br/>`ConsultarComprobante` | WSDL: `/soap/FacturacionService.svc?wsdl`<br/>Swagger: `/swagger` |
| **6** | **Auditoría y Bitácoras** | Propietario / .NET | VB.NET .NET 9 / CoreWCF | `8086` | `GET/POST/DELETE /api/v1/logs` | `RegistrarEvento`<br/>`ConsultarTotalEventos` | WSDL: `/soap/AuditoriaService.svc?wsdl`<br/>Swagger: `/swagger` |

---

## 3. Seguridad y Manejo de Errores Implementados

- **Capa REST:**
  - Token Bearer: `Authorization: Bearer test_token_2026`
  - API-Key: `X-API-Key: secret_key_cs`
  - Ante omisión o fallo: HTTP `401 Unauthorized` inmediato con payload JSON estandarizado.
- **Capa SOAP:**
  - HTTP Basic Auth: Usuario `admin` / Contraseña `admin_pass_2026` (`Authorization: Basic YWRtaW46YWRtaW5fcGFzc18yMDI2`)
  - O cabecera XML de seguridad `<soapenv:Header><AuthHeader>...`.
  - Ante fallo: HTTP `500` con `SOAP-ENV:Fault` estructurado (`soapenv:Client.AuthenticationFailed`).
- **CORS:**
  - Habilitado globalmente en los 6 servicios para permitir consumo directo desde clientes web.

---

## 4. Validaciones de Compilación y Sintaxis Realizadas

1. **Compilación C# (.NET 9):**
   - Comando: `dotnet build backend/csharp/FacturacionApi.csproj`
   - Resultado: **0 Errores. Compilación exitosa.**
2. **Compilación VB.NET (.NET 9):**
   - Comando: `dotnet build backend/vbnet/AuditoriaApi.vbproj`
   - Resultado: **0 Errores. Compilación exitosa con soporte CoreWCF.**
3. **Validación de Sintaxis Python:**
   - Comando: `python -m py_compile backend/python/app/*.py docs/test_suite.py`
   - Resultado: **0 Errores de sintaxis.**
4. **Validación de Sintaxis PHP:**
   - Comando: `php -l backend/php/public/index.php`, `Database.php`, `OrderService.php`, `SoapHandler.php`
   - Resultado: **0 Errores de sintaxis.**

---

## 5. Herramientas de Prueba y Visualización

- **Portal Web y Consola Interactiva:** `docs/index.html` permite interactuar con los 6 servicios directamente desde el navegador, alternar entre pruebas REST y SOAP XML, enviar credenciales y verificar tiempos de respuesta.
- **Suite de Pruebas Python:** `python docs/test_suite.py` ejecuta pruebas automatizadas de seguridad (401), operaciones REST (200), descarga de WSDL e invocación de contratos SOAP.
- **Script PowerShell:** `docs/test_all.ps1` para validación rápida en entornos Windows.
