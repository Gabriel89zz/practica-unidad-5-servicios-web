# Práctica Unidad 5: Sistema Distribuido de Comercio Electrónico y Logística
**Materia:** Programación en Ambiente Cliente-Servidor (7.° Semestre, Ingeniería en Informática)  
**Arquitectura:** Monorepo Multiplataforma con Microservicios Duales RESTful (JSON) y SOAP (XML/WSDL)

---

## 1. Visión General del Sistema

Este repositorio contiene la implementación completa de un ecosistema distribuido de 6 microservicios heterogéneos, diseñado bajo el caso de uso de **Comercio Electrónico y Logística**. Cada microservicio está desarrollado en un lenguaje y runtime diferente, combinando **4 tecnologías de Software Libre** y **2 tecnologías Empresariales / Propietarias de la plataforma .NET**.

Cada microservicio expone simultáneamente:
1. **API RESTful:** Serialización JSON, verbos estándar (GET, POST, PUT, DELETE), códigos de estado HTTP y documentación Swagger/OpenAPI.
2. **Servicio SOAP:** Formato XML, contrato WSDL funcional, operaciones tipadas y manejo de fallos mediante `SOAP-ENV:Fault`.
3. **Seguridad Unificada:** Validación de `Authorization: Bearer test_token_2026` o `X-API-Key: secret_key_cs` en REST, y credenciales `admin` / `admin_pass_2026` en SOAP.
4. **CORS Habilitado:** Configurado globalmente para permitir consumo directo desde clientes web.
5. **Empaquetado Docker:** Contenedorizado mediante `docker-compose.yml` para despliegue automatizado con un solo comando.

---

## 2. Catálogo de Microservicios, Tecnologías y Puertos

| # | Módulo | Categoría | Lenguaje / Stack | Framework REST | Motor SOAP | Puerto | Contrato WSDL / Documentación |
| :-: | :--- | :--- | :--- | :--- | :--- | :-: | :--- |
| **1** | **Catálogo e Inventario** | Software Libre | Java 21 / Spring Boot 3 | Spring MVC + Swagger | Spring-WS (`DefaultWsdl11Definition`) | `8081` | WSDL: `/ws/inventario.wsdl`<br/>Swagger: `/swagger-ui.html` |
| **2** | **Envíos y Rutas** | Software Libre | Python 3.12 / FastAPI | FastAPI + ReDoc | Spyne WSGI Application | `8082` | WSDL: `/soap/envios?wsdl`<br/>Swagger: `/docs` |
| **3** | **Gestión de Órdenes** | Software Libre | PHP 8.2+ / Slim 4 | Slim Framework 4 (Nyholm PSR-7) | PHP `SoapServer` nativo | `8083` | WSDL: `/soap/ordenes.wsdl`<br/>Swagger: `/docs` |
| **4** | **Notificaciones & Webhooks**| Software Libre | Ruby 3.2+ / Sinatra | Sinatra + Puma | Sinatra SOAP Dispatcher (Nokogiri) | `8084` | WSDL: `/soap/notificaciones/wsdl`<br/>Swagger: `/docs` |
| **5** | **Facturación y Timbrado** | Empresarial / .NET | C# (.NET 9) | ASP.NET Core Web API | CoreWCF (`CoreWCF.Http` / WSDL) | `8085` | WSDL: `/soap/FacturacionService.svc?wsdl`<br/>Swagger: `/swagger` |
| **6** | **Auditoría y Bitácoras** | Empresarial / .NET | VB.NET (.NET 9) | ASP.NET Core Web API en VB.NET | CoreWCF en VB.NET (`BasicHttpBinding`) | `8086` | WSDL: `/soap/AuditoriaService.svc?wsdl`<br/>Swagger: `/swagger` |

---

## 3. Estructura del Proyecto

```text
practica-unidad-5/
├── docker-compose.yml             # Orquestación de los 6 contenedores
├── .gitignore
├── README.md                      # Este manual general
├── backend/
│   ├── java/                      # Módulo 1: Java Spring Boot 3 (Catálogo e Inventario)
│   │   ├── Dockerfile
│   │   ├── pom.xml
│   │   └── src/main/...
│   ├── python/                    # Módulo 2: Python FastAPI + Spyne (Envíos y Rutas)
│   │   ├── Dockerfile
│   │   ├── requirements.txt
│   │   └── app/...
│   ├── php/                       # Módulo 3: PHP Slim 4 + SQLite (Gestión de Órdenes)
│   │   ├── Dockerfile
│   │   ├── composer.json
│   │   ├── public/index.php
│   │   └── src/...
│   ├── ruby/                      # Módulo 4: Ruby Sinatra + Puma (Notificaciones)
│   │   ├── Dockerfile
│   │   ├── Gemfile
│   │   ├── app.rb
│   │   └── wsdl/...
│   ├── csharp/                    # Módulo 5: C# ASP.NET Core + CoreWCF (Facturación)
│   │   ├── Dockerfile
│   │   ├── FacturacionApi.csproj
│   │   ├── Program.cs
│   │   ├── Controllers/...
│   │   └── Soap/...
│   ├── vbnet/                     # Módulo 6: VB.NET ASP.NET Core + CoreWCF (Auditoría)
│   │   ├── Dockerfile
│   │   ├── AuditoriaApi.vbproj
│   │   ├── Program.vb
│   │   ├── Controllers/...
│   │   └── Soap/...
│   └── shared/                    # Bibliotecas compartidas de interoperabilidad
│       └── WcfHelper/
└── docs/
    ├── ARCHITECTURE.md            # Justificación de diseño, diagramas y análisis SL vs Propietario
    ├── API_REFERENCE.md           # Catálogo detallado de todos los endpoints REST y ejemplos curl
    ├── SOAP_ENVELOPES.md          # Ejemplos de sobres XML de petición/respuesta para Postman/SoapUI
    ├── test_suite.py              # Suite de pruebas automatizadas en Python
    ├── test_all.ps1               # Script de pruebas automatizadas en PowerShell
    └── index.html                 # Portal web interactivo y consola de pruebas visual
```

---

## 4. Guía de Puesta en Marcha

### Opción A: Despliegue con Docker Compose (Recomendado)
Desde la raíz del proyecto, ejecute:

```bash
docker-compose up --build
```

Esto compilará las imágenes multi-etapa y levantará automáticamente los 6 servicios en sus respectivos puertos (`8081` a `8086`).

### Opción B: Ejecución Local Independiente
Cada microservicio puede ejecutarse de forma nativa en su respectivo runtime:

1. **Java (`backend/java`):**
   ```bash
   mvn spring-boot:run
   ```
2. **Python (`backend/python`):**
   ```bash
   pip install -r requirements.txt
   uvicorn app.main:app --host 0.0.0.0 --port 8082 --reload
   ```
3. **PHP (`backend/php`):**
   ```bash
   composer install
   php -S 0.0.0.0:8083 -t public
   ```
4. **Ruby (`backend/ruby`):**
   ```bash
   bundle install
   ruby app.rb
   ```
5. **C# (`backend/csharp`):**
   ```bash
   dotnet run --project FacturacionApi.csproj
   ```
6. **VB.NET (`backend/vbnet`):**
   ```bash
   dotnet run --project AuditoriaApi.vbproj
   ```

---

## 5. Pruebas y Validación del Sistema

### 1. Consola Web Interactiva (Dashboard)
Abra el archivo `docs/index.html` en cualquier navegador web (Chrome, Edge, Firefox). Esta interfaz gráfica le permite:
- Ver el estado de los 6 microservicios.
- Ejecutar peticiones REST en vivo (con o sin autenticación para validar respuestas 200 y 401).
- Transmitir peticiones SOAP XML en vivo y visualizar los sobres de respuesta en tiempo real.
- Acceder directamente a los Swagger UI y contratos WSDL.

### 2. Suite Automatizada en Python
Con los servicios en ejecución, ejecute:
```bash
python docs/test_suite.py
```

El script verificará automáticamente:
- Conectividad de los 6 microservicios.
- Rechazo HTTP 401 sin credenciales en endpoints protegidos.
- Respuesta HTTP 200 con Bearer Token en peticiones REST.
- Descarga y validez sintáctica de los 6 contratos WSDL.
- Invocación de operaciones SOAP con envoltorios XML.

### 3. Suite en PowerShell
En entornos Windows:
```powershell
.\docs\test_all.ps1
```

---

## 6. Autenticación y Credenciales

### REST
- Encabezado: `Authorization: Bearer test_token_2026`
- O alternativo: `X-API-Key: secret_key_cs`
- Fallo: HTTP `401 Unauthorized`.

### SOAP
- HTTP Basic Auth: Usuario `admin`, Contraseña `admin_pass_2026` (`Authorization: Basic YWRtaW46YWRtaW5fcGFzc18yMDI2`)
- O encabezado XML `<soapenv:Header><AuthHeader>...`
- Fallo: HTTP `500` con `SOAP-ENV:Fault` (`soapenv:Client.AuthenticationFailed`).
