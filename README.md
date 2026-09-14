# Práctica Unidad 5: Sistema Distribuido de Comercio Electrónico y Logística
**Materia:** Programación en Ambiente Cliente-Servidor (7.° Semestre, Ingeniería en Informática)  
**Arquitectura:** Monorepo Multiplataforma con Microservicios Duales RESTful (JSON) y SOAP (XML/WSDL)

> ### 🎓 Documentación Oficial y Reporte Académico
> **Alumno:** Hector Gabriel Torres Arzola  
> **Carrera:** Ingeniería en Informática — 7.° Semestre  
> **Servidor de Producción:** Debian GNU/Linux 13 (`192.168.1.176`)  
> 
> 📄 **[👉 HACER CLIC AQUÍ PARA ABRIR EL REPORTE COMPLETO DE PRÁCTICA FINAL (docs/REPORTE_PRACTICA_FINAL.md)](docs/REPORTE_PRACTICA_FINAL.md)**  
> 
> *El reporte incluye la resolución de todos los puntos solicitados con 14 capturas de pantalla de evidencia:*
> - *Topología de red, Docker y Firewall UFW (8081-8086).*
> - *Contratos formales WSDL y documentación interactiva OpenAPI / Swagger.*
> - *Seguridad perimetral (HTTP 401 Unauthorized y SOAP Fault).*
> - *Pruebas profesionales en Postman y SoapUI.*
> - *Demostración y flujo de datos en vivo de las 3 Aplicaciones Cliente (Web SPA, Desktop WinForms y CLI Terminal).*
> - *Tabla comparativa técnica y de rendimiento entre los 6 lenguajes.*

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
│   ├── python/                    # Módulo 2: Python FastAPI + Spyne (Envíos y Rutas)
│   ├── php/                       # Módulo 3: PHP Slim 4 + SQLite (Gestión de Órdenes)
│   ├── ruby/                      # Módulo 4: Ruby Sinatra + Puma (Notificaciones)
│   ├── csharp/                    # Módulo 5: C# ASP.NET Core + CoreWCF (Facturación)
│   ├── vbnet/                     # Módulo 6: VB.NET ASP.NET Core + CoreWCF (Auditoría)
│   └── shared/                    # Bibliotecas compartidas de interoperabilidad
├── clients/                       # Aplicaciones Cliente Heterogéneas (Multiplataforma)
│   ├── web-app/                   # Cliente 1: Web SPA (HTML5, Vanilla CSS Glassmorphism, JS ES6+)
│   │   ├── index.html             # Tienda en línea con carrito, inspector de red y cotizador
│   │   ├── styles.css             # Sistema de diseño responsivo y moderno
│   │   ├── app.js                 # Consumidor REST (Java, PHP) y SOAP (Java, PHP, Python)
│   │   └── README.md
│   ├── desktop-app/               # Cliente 2: Escritorio WinForms (.NET 9 C#)
│   │   ├── DesktopApp.csproj
│   │   ├── MainForm.cs            # Timbrado SAT, tracking logístico y monitor de tráfico
│   │   ├── Program.cs
│   │   └── README.md
│   └── cli-tool/                  # Cliente 3: Terminal / Consola CLI (Python 3)
│       ├── cli.py                 # Despacho de alertas (Ruby) y registro de auditoría (VB.NET)
│       └── README.md
├── scripts/                       # Scripts de automatización (.sh para Linux/Debian y .bat para Windows)
│   ├── check_services.sh / .bat   # Verificación de salud y puertos 8081-8086
│   ├── start_backend.sh           # Inicio de contenedores Docker en segundo plano
│   ├── stop_backend.sh            # Detención de contenedores
│   ├── run_web.sh / .bat          # Lanzador del Cliente Web SPA
│   ├── run_cli.sh / .bat          # Lanzador del Cliente de Consola CLI
│   ├── run_desktop.sh / .bat      # Lanzador del Cliente de Escritorio WinForms
│   └── run_tests.sh / .bat        # Ejecutor de la suite de pruebas automatizadas
├── run.sh                         # Menú interactivo unificado para Linux / Debian / macOS / WSL
├── run.bat                        # Menú interactivo unificado para Windows
└── docs/
    ├── REPORTE_PRACTICA_FINAL.md  # 📄 Reporte final académico completo con 14 evidencias
    ├── img/                       # Capturas de pantalla de evidencias (Docker, UFW, Postman, etc.)
    ├── ARCHITECTURE.md            # Justificación de diseño, diagramas y análisis SL vs Propietario
    ├── COMPARATIVA_SERVICIOS_WEB.md # Comparativa técnica exhaustiva entre los 6 lenguajes
    ├── API_REFERENCE.md           # Catálogo detallado de todos los endpoints REST y ejemplos curl
    ├── SOAP_ENVELOPES.md          # Ejemplos de sobres XML de petición/respuesta para Postman/SoapUI
    ├── EcoLogistics_Postman_Collection.json # Colección oficial de pruebas para Postman
    ├── EcoLogistics_SoapUI_Project.xml     # Proyecto oficial de pruebas para SoapUI
    ├── test_suite.py              # Suite de pruebas automatizadas en Python
    ├── test_all.ps1               # Script de pruebas automatizadas en PowerShell
    └── index.html                 # Portal web interactivo y consola de pruebas visual
```

---

## 4. Guía de Puesta en Marcha

### Opción 1: Menú Interactivo Todo-en-Uno (Más Rápido y Fácil)
Tanto en **Linux / Debian Server** como en **Windows**, puede ejecutar el menú interactivo que automatiza el inicio de los servicios, la verificación de salud y la ejecución de cada cliente:

* **En Linux / Debian Server / macOS:**
  ```bash
  ./run.sh
  ```
* **En Windows (PowerShell / CMD / Doble clic):**
  ```cmd
  run.bat
  ```

---

### Opción 2: Despliegue con Docker Compose
Desde la raíz del proyecto, ejecute:

```bash
docker compose up -d --build
# O mediante el script: ./scripts/start_backend.sh
```

Esto compilará las imágenes multi-etapa y levantará automáticamente los 6 servicios en sus respectivos puertos (`8081` a `8086`). Para detenerlos, ejecute `docker compose down` o `./scripts/stop_backend.sh`.

---

### Opción 3: Ejecución Local Independiente
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

## 5. Aplicaciones Cliente Multiplataforma (Web, Desktop, CLI)

Como parte de los requisitos de la práctica ("*Desarrollar aplicaciones cliente capaces de consumir las APIs mediante solicitudes HTTP y/o clientes SOAP*"), se implementaron tres aplicaciones cliente en plataformas y lenguajes totalmente distintos, asegurando una cobertura completa de los 6 microservicios del ecosistema:

```
                                  ┌──────────────────────────┐
                                  │      BACKEND ECOSYSTEM    │
                                  ├──────────────────────────┤
  ┌─────────────────────────┐     │  Java (:8081)            │
  │   1. Web SPA Client     │────▶│    REST / SOAP             │
  │ (HTML5/Vanilla CSS/JS)  │────▶│  PHP (:8083)             │
  └─────────────────────────┘     │    REST / SOAP             │
                                  │                          │
  ┌─────────────────────────┐     │  Python (:8082)          │
  │  2. Desktop WinForms    │────▶│    REST / SOAP             │
  │      (.NET 9 C#)        │────▶│  C# (.NET 9) (:8085)     │
  └─────────────────────────┘     │    REST / SOAP             │
                                  │                          │
  ┌─────────────────────────┐     │  Ruby (:8084)            │
  │    3. Terminal / CLI    │────▶│    REST / SOAP             │
  │       (Python 3)        │────▶│  VB.NET (:8086)          │
  └─────────────────────────┘     │    REST / SOAP             │
                                  └──────────────────────────┘
```

### 1. Cliente Web (`clients/web-app/`)
* **Tecnología:** HTML5 semántico, Vanilla CSS (diseño responsivo dark glassmorphism con tipografías Google Fonts `Outfit`, `Inter` y `JetBrains Mono`), JavaScript ES6+ asíncrono (`fetch`).
* **Servicios Consumidos:**
  * **Java Spring Boot (:8081):** REST (`GET /api/v1/productos`) para renderizar el catálogo reactivo; SOAP (`POST /ws`, `ConsultarStockRequest`) para verificar existencias en almacén en tiempo real.
  * **PHP Slim 4 (:8083):** REST (`POST /api/v1/ordenes`) para registrar pedidos; SOAP (`POST /soap/ordenes`, `ValidarEstadoOrden`) para validación de estado de órdenes.
  * **Python FastAPI / Spyne (:8082):** SOAP (`POST /soap/envios`, `CalcularTarifa`) para el cotizador interactivo de envíos.
* **Características:** Selector de host para alternar entre `localhost` y la IP del servidor Debian, carrito de compras deslizable, notificaciones tipo Toast y **Live Network Protocol Inspector** integrado en el pie de página para inspeccionar cabeceras, latencia y payloads JSON/XML en tiempo real.
* **Cómo ejecutarlo:**
  ```bash
  # Mediante script automatizado:
  ./scripts/run_web.sh        # En Linux / Debian / macOS / Git Bash
  scripts\run_web.bat         # En Windows CMD / PowerShell

  # O directamente sirviéndolo con Python:
  python -m http.server 3000 --directory clients/web-app
  ```

### 2. Cliente de Escritorio (`clients/desktop-app/`)
* **Tecnología:** C# con .NET 9 WinForms (`net9.0-windows`), `HttpClient` nativo y `System.Xml.Linq` (LINQ to XML).
* **Servicios Consumidos:**
  * **C# ASP.NET Core / CoreWCF (:8085):** SOAP (`POST /soap/FacturacionService.svc`, `TimbrarComprobante`) con construcción de XML estructurado y timbrado fiscal; REST (`GET /api/v1/facturas`) con mapeo en `DataGridView`.
  * **Python FastAPI / Spyne (:8082):** SOAP (`POST /soap/envios`, `CalcularTarifa`) para cotización de fletes; REST (`GET /api/v1/envios/{guia}`) para rastreo de guías logísticas.
* **Características:** Pestañas para Facturación Electrónica SAT y Logística, selector dinámico de servidor remoto, visor de respuestas y terminal de depuración de protocolos con tiempos de respuesta.
* **Cómo ejecutarlo:**
  ```bash
  # Mediante script automatizado:
  ./scripts/run_desktop.sh    # En Linux / Debian (valida compilación) o WSLg / Windows
  scripts\run_desktop.bat     # En Windows CMD / PowerShell

  # O directamente con dotnet CLI:
  dotnet run --project clients/desktop-app/DesktopApp.csproj
  ```

### 3. Cliente de Terminal / CLI (`clients/cli-tool/`)
* **Tecnología:** Python 3 nativo utilizando únicamente la biblioteca estándar (`urllib.request`, `xml.etree.ElementTree`, `json`, `argparse`). **Cero dependencias externas (`pip install` no requerido)**.
* **Servicios Consumidos:**
  * **Ruby Sinatra (:8084):** REST (`GET /api/v1/destinatarios`) para consultar destinatarios de alertas; SOAP (`POST /soap/notificaciones`, `DespacharAlertaCritica`) para emitir alertas de incidentes con autenticación Basic Auth.
  * **VB.NET ASP.NET Core / CoreWCF (:8086):** REST (`GET /api/v1/logs`) para consultar bitácoras de auditoría; SOAP (`POST /soap/AuditoriaService.svc`, `RegistrarEvento`) para asentar entradas de trazabilidad y seguridad.
* **Características:** Menú interactivo guiado por consola con colores ANSI o modo directo por flags de línea de comandos (`--service`, `--accion`, `--mensaje`), soporte dinámico para `--host` (apuntando al servidor Debian).
* **Cómo ejecutarlo:**
  ```bash
  # Mediante script automatizado (modo interactivo):
  ./scripts/run_cli.sh        # En Linux / Debian
  scripts\run_cli.bat         # En Windows

  # O con parámetros directos apuntando a su servidor remoto:
  ./scripts/run_cli.sh http://TU_IP_DEBIAN
  python clients/cli-tool/cli.py --host http://TU_IP_DEBIAN --service vbnet --soap --accion "DESPLIEGUE" --mensaje "Servicios actualizados"
  ```

---

## 6. Pruebas y Validación del Sistema

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

## 7. Autenticación y Credenciales

### REST
- Encabezado: `Authorization: Bearer test_token_2026`
- O alternativo: `X-API-Key: secret_key_cs`
- Fallo: HTTP `401 Unauthorized`.

### SOAP
- HTTP Basic Auth: Usuario `admin`, Contraseña `admin_pass_2026` (`Authorization: Basic YWRtaW46YWRtaW5fcGFzc18yMDI2`)
- O encabezado XML `<soapenv:Header><AuthHeader>...`
- Fallo: HTTP `500` con `SOAP-ENV:Fault` (`soapenv:Client.AuthenticationFailed`).

---

## 8. Entregables y Reporte de Práctica Final

Toda la documentación académica solicitada en la rúbrica se encuentra consolidada con sus respectivas evidencias fotográficas y comparativas en los siguientes documentos:

| Documento | Descripción / Contenido Principal |
| :--- | :--- |
| **[📄 docs/REPORTE_PRACTICA_FINAL.md](docs/REPORTE_PRACTICA_FINAL.md)** | **Reporte Oficial Completo:** Problema real, arquitectura, APIs REST, servicios SOAP WSDL, seguridad HTTP 401, Postman, SoapUI, 3 aplicaciones cliente y 14 capturas de pantalla de evidencia. |
| **[📊 docs/COMPARATIVA_SERVICIOS_WEB.md](docs/COMPARATIVA_SERVICIOS_WEB.md)** | **Comparativa Técnica Exhaustiva:** Matriz detallada entre los 6 lenguajes (sintaxis, facilidad, latencias REST/SOAP, consumo de RAM, interoperabilidad y seguridad). |
| **[🏛️ docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** | **Arquitectura del Sistema:** Topología de red, orquestación Docker, diagramas de flujo y justificación técnica de software libre vs propietario. |
| **[📬 docs/EcoLogistics_Postman_Collection.json](docs/EcoLogistics_Postman_Collection.json)** | **Colección de Postman:** Peticiones REST y sobres SOAP XML preconfigurados listos para importar. |
| **[🧼 docs/EcoLogistics_SoapUI_Project.xml](docs/EcoLogistics_SoapUI_Project.xml)** | **Proyecto de SoapUI:** Contratos WSDL y operaciones SOAP listos para ejecutar pruebas formales. |

