# Arquitectura del Sistema Distribuido de Comercio Electrónico y Logística
**Materia:** Programación en Ambiente Cliente-Servidor (7.° Semestre, Ingeniería en Informática)  
**Nivel:** Arquitectura de Sistemas y Servicios Web Heterogéneos  

---

## 1. Visión General del Ecosistema

El sistema representa la infraestructura backend distribuida de una empresa moderna de comercio electrónico y operaciones logísticas. Para cumplir con el propósito pedagógico y técnico de la materia, la arquitectura integra intencionalmente **seis entornos de ejecución y lenguajes de programación diferentes**, divididos equitativamente entre **Software Libre** y **Plataformas Empresariales / Propietarias (.NET)**.

Cada subsistema opera de forma independiente y expone simultáneamente dos paradigmas de comunicación:
1. **RESTful API (JSON):** Orientada a clientes web interactivos, aplicaciones móviles y frontends modernos que requieren operaciones CRUD ligeras e interoperabilidad sin estado.
2. **SOAP Web Service (XML / WSDL):** Orientado a transacciones formales, validación estricta de contratos mediante esquemas XSD y protocolos empresariales de integración (B2B, timbrado fiscal, despacho de alertas críticas).

---

## 2. Diagrama de Arquitectura y Flujo de Integración

```mermaid
sequenceDiagram
    autonumber
    actor Cliente as Cliente Web / Comprador
    participant ModJava as [Java :8081] Catálogo e Inventario
    participant ModPHP as [PHP :8083] Gestión de Órdenes
    participant ModPython as [Python :8082] Envíos y Rutas
    participant ModCSharp as [C# :8085] Facturación y Timbrado
    participant ModRuby as [Ruby :8084] Notificaciones y Webhooks
    participant ModVB as [VB.NET :8086] Auditoría y Bitácoras

    Note over Cliente,ModJava: Fase 1: Selección y Reserva
    Cliente->>ModJava: REST GET /api/v1/productos (Consulta catálogo)
    ModJava-->>Cliente: Lista JSON de productos con stock disponible
    Cliente->>ModPHP: REST POST /api/v1/ordenes (Crea orden de compra)
    ModPHP-->>Cliente: Orden creada (ORD-2026-XXXX) con desglose de totales
    ModPHP->>ModJava: SOAP ConsultarStockRequest / ActualizarStockRequest
    ModJava-->>ModPHP: SOAP ConsultarStockResponse (Stock reservado)

    Note over Cliente,ModPython: Fase 2: Logística y Despacho
    ModPHP->>ModPython: SOAP CalcularTarifa(origen, destino, peso, servicio)
    ModPython-->>ModPHP: TarifaResult (Costo y tiempo estimado de entrega)
    ModPHP->>ModPython: REST POST /api/v1/envios (Genera guía de paquetería)
    ModPython-->>ModPHP: EnvioResponse (Número de guía GUIA-2026-XXXX)

    Note over ModPHP,ModCSharp: Fase 3: Transacción Fiscal
    ModPHP->>ModCSharp: SOAP TimbrarComprobante(RFC, Subtotal, IVA, Total)
    ModCSharp-->>ModPHP: ComprobanteFiscal (Folio Fiscal UUID SAT + Sello Digital)

    Note over ModPHP,ModRuby: Fase 4: Notificación Multicanal
    ModPHP->>ModRuby: SOAP DespacharAlertaCritica(ORDEN_CONFIRMADA, canal, destinatario)
    ModRuby-->>ModPHP: ResultadoDespacho (Mensaje encolado / entregado)

    Note over ModJava,ModVB: Fase 5: Trazabilidad y Auditoría Atómica
    par Registro Transaccional
        ModJava->>ModVB: SOAP RegistrarEvento(Java, "VENTA_PRODUCTO", ...)
        ModPHP->>ModVB: SOAP RegistrarEvento(PHP, "CREAR_ORDEN", ...)
        ModPython->>ModVB: SOAP RegistrarEvento(Python, "GENERAR_GUIA", ...)
        ModCSharp->>ModVB: SOAP RegistrarEvento(CSharp, "TIMBRAR_CFDI", ...)
        ModRuby->>ModVB: SOAP RegistrarEvento(Ruby, "DESPACHAR_ALERTA", ...)
    end
    ModVB-->>ModVB: Eventos persistidos con hash y timestamp UTC
```

---

## 3. Matriz Comparativa: Software Libre vs. Plataforma Empresarial Propietaria

| Eje de Comparación | Software Libre (Java, Python, PHP, Ruby) | Plataforma Empresarial / Propietaria (C# .NET, VB.NET) |
| :--- | :--- | :--- |
| **Licenciamiento y Costos** | Código abierto (Apache 2.0, MIT, PSF, PHP License). Sin costo de licenciamiento de servidor ni de compiladores. | .NET es open-source bajo Microsoft (.NET Foundation), pero históricamente anclado al ecosistema Windows Server, SQL Server y herramientas Visual Studio Enterprise. |
| **Gobernanza y Evolución** | Conducida por fundaciones comunitarias y consorcios abiertos (Eclipse Foundation, Python Software Foundation, Ruby Association). | Conducida primordialmente por Microsoft Corp, con fuerte enfoque en estabilidad hacia atrás corporativa y estándares empresariales. |
| **Rendimiento y Runtimes** | - **Java:** JVM altamente optimizada para alto rendimiento transaccional concurrente.<br/>- **Python/Ruby:** Runtimes interpretados, ideales para desarrollo ágil y scripts dinámicos.<br/>- **PHP:** Motor optimizado para procesamiento por petición web sin estado. | - **CLR (.NET):** JIT de altísimo rendimiento, compilación nativa AOT disponible, gestión de memoria de baja latencia con garbage collector por servidor. |
| **Soporte SOAP / Contratos** | - Spring-WS genera WSDL formal a partir de esquemas XSD nativos.<br/>- Spyne implementa protocolos RPC mediante metaprogramación.<br/>- PHP cuenta con módulo nativo C (`ext-soap`).<br/>- Ruby procesa XML estructurado con parser C-Nokogiri. | - **CoreWCF:** Implementación oficial de nivel empresarial de los estándares WS-* (WS-Security, WS-Addressing, WS-ReliableMessaging), interoperable con sistemas bancarios y gubernamentales heredados. |
| **Idoneidad en la Práctica** | Excelente para servicios perimetrales, APIs REST públicas, scripts de automatización e integraciones ligeras. | Excelente para servicios centrales de negocio, facturación fiscal de alta criticidad, auditorías normativas y orquestación empresarial. |

---

## 4. Comparación de Paradigmas: RESTful vs. SOAP

```text
+-----------------------------------------------------------------------------------------------+
| CRITERIO               | RESTful (Representational State Transfer) | SOAP (Simple Object Access Protocol)      |
+------------------------+-------------------------------------------+-------------------------------------------+
| Protocolo Subyacente   | Estrictamente HTTP/HTTPS (GET, POST, etc) | Agnóstico: HTTP, SMTP, TCP, JMS, etc.     |
| Formato de Mensajería  | JSON (liviano, legible, sin metadatos)   | XML Envelope (Header + Body + Fault)      |
| Contrato Formal        | Opcional / Descriptivo (OpenAPI / Swagger)| Obligatorio / Prescriptivo (WSDL + XSD)   |
| Estado                 | Sin estado (Stateless)                    | Puede mantener estado o sesiones WS-*     |
| Manejo de Errores      | Códigos HTTP estándar (400, 401, 404, 500)| SOAP Fault estructurado (<faultcode>)     |
| Rendimiento            | Muy alto (bajo overhead de serialización) | Moderado (parseo de XML y validación XSD) |
| Caso de Uso Principal  | Aplicaciones SPA, móviles, CRUD ágiles    | B2B, banca, satélite fiscal, transacciones|
+-----------------------------------------------------------------------------------------------+
```

---

## 5. Arquitectura de Seguridad

La solución implementa una estrategia de **Defensa en Profundidad (Defense in Depth)** con mecanismos simétricos según el protocolo de transporte:

### A. Capa REST (Tokens de Acceso)
- Cada microservicio intercepta las solicitudes entrantes en `/api/**`.
- Verifica la presencia y validez de:
  - `Authorization: Bearer test_token_2026`
  - o encabezado alternativo `X-API-Key: secret_key_cs`.
- En caso de omisión o credencial no coincidente:
  - Se interrumpe la cadena de ejecución inmediatamente.
  - Se retorna HTTP `401 Unauthorized` con cabecera `WWW-Authenticate: Bearer` y cuerpo JSON estandarizado:
    ```json
    {
      "error": "Unauthorized",
      "message": "Token Bearer o API-Key inválida o ausente.",
      "statusCode": 401,
      "timestamp": "2026-09-11T19:30:00Z"
    }
    ```

### B. Capa SOAP (Seguridad en Mensaje y Transporte)
- Los endpoints SOAP validan:
  1. Autenticación HTTP Basic (`Authorization: Basic YWRtaW46YWRtaW5fcGFzc18yMDI2`), correspondiente al usuario `admin` y contraseña `admin_pass_2026`.
  2. O cabecera de autenticación XML `<soapenv:Header><AuthHeader>...` dentro del propio sobre SOAP.
- Si las credenciales fallan:
  - El servicio genera un elemento `<soapenv:Fault>` con código `soapenv:Client.AuthenticationFailed`.
  - Se acompaña de un mensaje descriptivo y un código de error específico en el nodo `<detail>`.

---

## 6. Persistencia Autocontenida y Resiliencia

Para asegurar un despliegue inmediato sin requerir configuración previa de motores de bases de datos pesados (Oracle, MS SQL o PostgreSQL externos):
- **Almacenamiento en Memoria Thread-Safe:**
  - Java: `ConcurrentHashMap<Long, Producto>` y `AtomicLong`.
  - Python: `threading.Lock()` protegiendo diccionarios de envíos.
  - Ruby: `Mutex.synchronize` administrando arreglos de destinatarios y alertas.
  - C# y VB.NET: `ConcurrentDictionary(Of TKey, TValue)` administrando colecciones de facturas y registros de auditoría.
- **SQLite Embebido:**
  - PHP: SQLite nativo via PDO (`storage/ordenes.db`) con creación automática de esquema y datos semilla en la primera ejecución.

---

## 7. Orquestación mediante Contenedores (Docker Compose)

El archivo `docker-compose.yml` orquesta los 6 microservicios bajo la red virtual `ecommerce-net`:
- Todos los servicios exponen sus puertos directamente al host (`8081` a `8086`).
- Políticas de reinicio `unless-stopped`.
- Imágenes optimizadas mediante compilación en varias etapas (**Multi-Stage Builds**) para Java, C# y VB.NET, reduciendo el tamaño de imagen final a solo el runtime mínimo necesario.
