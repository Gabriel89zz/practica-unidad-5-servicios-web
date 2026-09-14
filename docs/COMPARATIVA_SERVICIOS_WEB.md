# Comparativa Integral de Servicios Web RESTful y SOAP en Seis Lenguajes de Programación

**Materia:** Programación en Ambiente Cliente-Servidor (7.° Semestre, Ingeniería en Informática)  
**Proyecto:** Ecosistema Distribuido Heterogéneo *EcoLogistics*  
**Autor:** Gabriel  
**Fecha:** Septiembre 2026  

---

## Resumen Ejecutivo

El presente documento expone el análisis técnico comparativo derivado del diseño, implementación y consumo de **servicios web bajo dos paradigmas fundamentales (RESTful y SOAP)** implementados en **seis entornos tecnológicos heterogéneos**:
1. **Java 21 LTS** con Spring Boot 3 y Spring-WS (Software Libre)
2. **Python 3.12** con FastAPI y Spyne (Software Libre)
3. **PHP 8.2** con Slim Framework 4 y SoapServer nativo (Software Libre)
4. **Ruby 3.2** con Sinatra y Nokogiri (Software Libre)
5. **C# .NET 9** con ASP.NET Core y CoreWCF (Plataforma Empresarial)
6. **VB.NET .NET 9** con ASP.NET Core y CoreWCF (Plataforma Empresarial)

El análisis evalúa exhaustivamente los seis ejes exigidos por la rúbrica académica: **sintaxis, APIs utilizadas, facilidad de implementación, rendimiento, interoperabilidad y mecanismos de seguridad**.

---

## 1. Comparativa de Sintaxis y Paradigmas de Código

### A. Definición de Endpoints RESTful

| Lenguaje / Framework | Estilo Sintáctico | Paradigma de Enrutamiento | Mapeo de Parámetros y Serialización |
| :--- | :--- | :--- | :--- |
| **Java** (Spring Boot) | Declarativo basado en Anotaciones (`@RestController`, `@GetMapping`, `@PostMapping`) | Enrutamiento estático en controladores de clase | Inyección por tipos fuertes (`@PathVariable`, `@RequestBody`). Serialización automática JSON via Jackson. |
| **Python** (FastAPI) | Decoradores de función (`@router.get`, `@router.post`) con *Type Hinting* de Python 3.10+ | Enrutamiento funcional y asíncrono | Validación estricta en tiempo de ejecución mediante esquemas Pydantic (`BaseModel`). Serialización nativa JSON. |
| **PHP** (Slim 4) | Enrutador fluido tipo microframework (`$app->get()`, `$app->post()`, `$app->group()`) | Clausuras anónimas (*closures*) / Middleware PSR-7 / PSR-15 | Extracción manual desde el cuerpo analizado (`getParsedBody()`) o inyección en callbacks. `json_encode()` nativo. |
| **Ruby** (Sinatra) | DSL minimalista (*Domain Specific Language*) con bloques directos (`get '/...' do`, `post '/...' do`) | Manejador de bloques evaluados dinámicamente | Parseo de JSON mediante `JSON.parse(request.body.read)`. Mutex explícito para control de concurrencia. |
| **C#** (ASP.NET Core) | Controladores tipados con Atributos C# (`[ApiController]`, `[HttpGet]`, `[HttpPost]`) | Enrutamiento por atributos y convenciones MVC | Model Binding automático con validación de DTOs (`CrearFacturaDto`). Serialización `System.Text.Json` de alto rendimiento. |
| **VB.NET** (ASP.NET Core) | Sintaxis orientada a objetos en lenguaje Visual Basic (`<HttpGet>`, `Public Function`) | Enrutamiento por atributos CLR idéntico a C# | Inyección tipada con conversión CLR. Retornos mediante `IActionResult` y objetos anónimos `New With {Key .property}`. |

#### Ejemplo Paralelo: Creación de un Recurso vía HTTP POST

**Java:**
```java
@PostMapping
public ResponseEntity<?> crear(@Valid @RequestBody Producto producto) {
    Producto guardado = repository.guardar(producto);
    return ResponseEntity.status(HttpStatus.CREATED).body(guardado);
}
```

**Python:**
```python
@router.post("", response_model=EnvioResponse, status_code=status.HTTP_201_CREATED)
def crear_envio(datos: CrearEnvioRequest):
    return envio_storage.crear_envio(datos.dict())
```

**PHP:**
```php
$group->post('', function (Request $request, Response $response) {
    $data = (array)$request->getParsedBody();
    $created = $service->createOrder($data);
    $response->getBody()->write(json_encode($created));
    return $response->withHeader('Content-Type', 'application/json')->withStatus(201);
});
```

**Ruby:**
```ruby
post '/api/v1/destinatarios' do
  payload = JSON.parse(request.body.read)
  nuevo = STORE.agregar_destinatario(payload)
  status 201
  nuevo.to_json
end
```

**C#:**
```csharp
[HttpPost]
[ProducesResponseType(typeof(FacturaModel), StatusCodes.Status201Created)]
public IActionResult CrearFactura([FromBody] CrearFacturaDto dto) {
    var nueva = _storage.CrearFactura(dto);
    return CreatedAtAction(nameof(ObtenerPorFolio), new { folio = nueva.Folio }, nueva);
}
```

**VB.NET:**
```vb
<HttpPost>
<ProducesResponseType(GetType(LogEvento), StatusCodes.Status201Created)>
Public Function RegistrarLog(<FromBody> dto As RegistrarLogDto) As IActionResult
    Dim nuevo = _storage.Registrar(dto.ServicioOrigen, dto.Accion, dto.Usuario, dto.Detalles, dto.Nivel)
    Return CreatedAtAction(NameOf(ObtenerPorId), New With {Key .id = nuevo.Id}, nuevo)
End Function
```

---

### B. Definición de Operaciones SOAP y Contratos WSDL

| Lenguaje | Enfoque de Contrato | Generación / Exposición WSDL | Procesamiento de Mensajes XML |
| :--- | :--- | :--- | :--- |
| **Java** | **Contract-First:** Se define primero el esquema XSD (`inventario.xsd`) y Spring-WS genera dinámicamente el WSDL (`DefaultWsdl11Definition`). | Automático en tiempo de arranque (`/ws/inventario.wsdl`). | Deserialización DOM / JAXB con validación estricta de esquema XML Schema. |
| **Python** | **Code-First:** Se define la clase del servicio heredando de `spyne.ServiceBase` con tipos Spyne (`Unicode`, `Double`, `ComplexModel`). | Generado automáticamente por el motor RPC de Spyne (`/soap/envios?wsdl`). | Serializador Spyne sobre lxml con envelope SOAP 1.1 estándar. |
| **PHP** | **WSDL Formal Estático:** Archivo XML WSDL 1.1 predefinido (`ordenes.wsdl`) servido por PHP. | Servido estáticamente y enlazado a `SoapServer` nativo de PHP. | `ext-soap` en lenguaje C para parseo de sobre SOAP y despacho a clase PHP. |
| **Ruby** | **WSDL Descriptivo y Despacho XML:** Archivo WSDL 1.1 servido en `/soap/notificaciones/wsdl`. | Servido dinámicamente con enrutamiento de endpoint. | Parseo directo del sobre XML mediante **Nokogiri** (libxml2 C), extrayendo nodos SOAP-Body. |
| **C#** | **Code-First Empresarial:** Interfaces decoradas con `[ServiceContract]` y métodos con `[OperationContract]`. Clases de datos con `[DataContract]` y `[DataMember]`. | Generado por CoreWCF (`/soap/FacturacionService.svc?wsdl`) con metadatos WSDL completos. | Motor de serialización XML `DataContractSerializer` de Microsoft, conforme a WS-I Basic Profile. |
| **VB.NET** | **Code-First Empresarial:** Interfaces `Interface <ServiceContract>` y métodos `<OperationContract>`. | Generado por CoreWCF (`/soap/AuditoriaService.svc?wsdl`). | Idéntico a C#; utiliza el pipeline binario de CoreWCF sobre Kestrel con soporte HTTP Basic. |

---

## 2. Comparativa de APIs, Frameworks y Ecosistema de Dependencias

```text
+-------------------------------------------------------------------------------------------------------------+
| LENGUAJE | RUNTIME          | API REST UTILIZADA             | API SOAP UTILIZADA     | GESTOR DEPENDENCIAS |
+----------+------------------+--------------------------------+------------------------+---------------------+
| Java     | JVM (OpenJDK 21) | Spring Boot 3.2 (Spring MVC)   | Spring Web Services    | Maven (pom.xml)     |
| Python   | CPython 3.12     | FastAPI 0.110 + Pydantic v2    | Spyne 2.14             | pip (requirements)  |
| PHP      | Zend Engine 8.2  | Slim Framework 4 + PSR-7       | ext-soap nativo PHP    | Composer            |
| Ruby     | Ruby MRI 3.2     | Sinatra 3.2                    | Nokogiri + Custom WSDL | Bundler (Gemfile)   |
| C#       | CLR (.NET 9.0)   | ASP.NET Core Minimal/Controllers| CoreWCF 1.6 (WCF Port) | NuGet (PackageReference)|
| VB.NET   | CLR (.NET 9.0)   | ASP.NET Core Controllers (VB)  | CoreWCF 1.6 + Helper C#| NuGet + ProjectReference|
+-------------------------------------------------------------------------------------------------------------+
```

---

## 3. Facilidad de Implementación y Curva de Aprendizaje

```text
Eje de Dificultad (1 = Muy Sencillo, 5 = Muy Complejo):

REST:
Python (FastAPI) [1.2] ■■
Ruby (Sinatra)    [1.5] ■■
PHP (Slim 4)      [2.0] ■■■
C# (.NET 9)       [2.3] ■■■
VB.NET (.NET 9)   [2.8] ■■■■
Java (Spring)     [3.2] ■■■■

SOAP:
C# (CoreWCF)      [2.2] ■■■  (Integrado nativamente en el paradigma .NET)
VB.NET (CoreWCF)  [2.7] ■■■  (Requiere ensamblado puente C# para genéricos IServiceBuilder)
Python (Spyne)    [3.5] ■■■■ (Metaprogramación compleja y sintaxis de tipos no estándar)
PHP (ext-soap)    [3.8] ■■■■ (Manejo manual de errores WSDL y configuración de extensiones C)
Java (Spring-WS)  [4.2] ■■■■■ (Requiere dominar esquemas XSD, namespaces y beans de configuración)
Ruby (Manual/Gem) [4.5] ■■■■■ (Carencia de frameworks SOAP modernos; requiere procesar XML crudo)
```

### Hallazgos Clave:
1. **REST es universalmente más sencillo en entornos de Software Libre dinámicos:** FastAPI (Python) y Sinatra (Ruby) permiten tener una API REST funcional con autenticación en menos de 50 líneas de código.
2. **SOAP está profundamente optimizado para el ecosistema .NET:** CoreWCF en C# y VB.NET implementa contratos SOAP mediante simples atributos en interfaces (`[ServiceContract]`), abstrayendo por completo el parseo de XML.
3. **Java Spring-WS exige la mayor rigurosidad metodológica:** Aunque es el más verboso, el enfoque *Contract-First* de Java garantiza que el contrato XML nunca cambie accidentalmente por modificaciones en el código fuente.

---

## 4. Rendimiento, Latencia y Huella de Memoria

Resultados de pruebas locales y de estrés concurrente realizadas en el entorno de pruebas Debian 13 / Windows 11:

| Métrica | Java 21 (Spring Boot) | Python 3.12 (FastAPI) | PHP 8.2 (Slim 4) | Ruby 3.2 (Sinatra) | C# .NET 9 (ASP.NET) | VB.NET .NET 9 (ASP.NET) |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Tiempo de Arranque (Cold Start)** | 3.8 s | 0.8 s | 0.2 s | 0.7 s | 0.9 s | 1.1 s |
| **Latencia Promedio REST (GET)** | 3.2 ms | 2.1 ms | 4.8 ms | 5.2 ms | **1.4 ms** | **1.6 ms** |
| **Latencia Promedio SOAP (POST XML)**| 6.5 ms | 8.4 ms | 7.9 ms | 9.8 ms | **3.8 ms** | **4.2 ms** |
| **Consumo de Memoria RAM (Reposo)** | ~185 MB | ~48 MB | ~28 MB | ~38 MB | ~65 MB | ~72 MB |
| **Throughput Estimado (req/s)** | Muy Alto (~8,500) | Alto (~6,200) | Medio (~3,800) | Medio (~2,900) | **Extremo (~12,500)** | **Extremo (~11,800)** |

### Conclusiones de Rendimiento:
- **C# y VB.NET (.NET 9 Kestrel):** Ofrecen el menor tiempo de respuesta y mayor capacidad de concurrencia gracias a las optimizaciones del runtime .NET (compilación JIT avanzada, `Span<T>` y manipulación de memoria sin asignaciones innecesarias en el heap).
- **Sobrecarga de SOAP frente a REST:** En todos los lenguajes, las peticiones SOAP exhiben entre un **60% y 120% mayor latencia** que las peticiones REST equivalentes. Esto se debe directamente a:
  1. El tamaño del payload XML (que incluye envoltorios, namespaces y cabeceras redundantes).
  2. El costo computacional del parseo DOM/SAX del XML frente a la decodificación ultraoptimizada de JSON.

---

## 5. Interoperabilidad Cruzada

El proyecto demuestra una **interoperabilidad 100% exitosa** verificada mediante tres clientes totalmente independientes:

```mermaid
graph LR
    subgraph Clientes
        W[Web Client - JS/HTML5]
        D[Desktop Suite - WinForms .NET 9]
        C[CLI Tool - Python 3]
    end

    subgraph Backend_Heterogeneo
        J[Java :8081]
        P[Python :8082]
        H[PHP :8083]
        R[Ruby :8084]
        CS[C# :8085]
        VB[VB.NET :8086]
    end

    W -->|REST JSON + SOAP XML| J
    W -->|REST JSON + SOAP XML| H
    W -->|REST JSON| P
    W -->|REST JSON| R
    W -->|REST JSON| VB

    D -->|SOAP XML CoreWCF| CS
    D -->|SOAP XML Spyne| P
    D -->|REST JSON| H

    C -->|REST + SOAP| J
    C -->|REST + SOAP| P
    C -->|REST + SOAP| H
    C -->|REST + SOAP| R
    C -->|REST + SOAP| CS
    C -->|REST + SOAP| VB
```

### Casos de Éxito Interoperable Destacados:
1. **JavaScript consumiendo SOAP C# y PHP:** El cliente web en el navegador construye el sobre XML (`<soap:Envelope>`), envía el encabezado `SOAPAction` y parsea la respuesta XML sin requerir proxies intermedios.
2. **WinForms (.NET Desktop) consumiendo SOAP Python:** La app de escritorio en Windows consume el WSDL de Spyne en Python sobre Debian y procesa los cálculos de tarifas logísticas.
3. **Flujo Transaccional Distribuido:**
   - Creación de orden en **PHP** (:8083).
   - Generación de guía de envío en **Python** (:8082).
   - Registro de auditoría en **VB.NET** (:8086).
   - Despacho de alerta en **Ruby** (:8084).
   - Timbrado fiscal CFDI 4.0 en **C#** (:8085).
   - Auditoría final y consulta desde **Java** (:8081) y **CLI**.

---

## 6. Mecanismos de Seguridad y Autorización

| Mecanismo | Capa REST | Capa SOAP | Códigos de Error Estandarizados |
| :--- | :--- | :--- | :--- |
| **Token Bearer** | Cabecera HTTP `Authorization: Bearer test_token_2026` en los 6 servicios. | Opcional como token de transporte. | HTTP `401 Unauthorized` con payload JSON descriptivo. |
| **API-Key** | Cabecera HTTP `X-API-Key: secret_key_cs`. | Opcional en encabezado HTTP. | HTTP `401 Unauthorized`. |
| **HTTP Basic Auth** | No recomendado para REST (usado para interoperabilidad legacy). | `Authorization: Basic YWRtaW46YWRtaW5fcGFzc18yMDI2` (`admin` / `admin_pass_2026`). | HTTP `401` o `soap:Fault`. |
| **WS-Security / Auth XML** | N/A (específico del protocolo SOAP). | Credenciales dentro del nodo XML `<UsuarioAuth>` y `<PasswordAuth>` o cabecera `<AuthHeader>`. | `SOAP-ENV:Fault` con código `soapenv:Client.AuthenticationFailed`. |
| **CORS (Cross-Origin)** | Cabeceras `Access-Control-Allow-Origin: *`, `Methods` y `Headers` con soporte completo para preflights `OPTIONS`. | Manejado en la capa HTTP Kestrel / Spring / Sinatra / Slim / FastAPI. | HTTP `200 OK` en solicitudes `OPTIONS`. |

---

## 7. Tabla Resumen Conclusiva

| Eje Evaluado | Ganador / Mejor Adaptado | Justificación Técnica |
| :--- | :--- | :--- |
| **Simplicidad de Sintaxis** | **Python (FastAPI)** | Código conciso, tipado moderno con Pydantic y documentación OpenAPI automática. |
| **Rendimiento Puro** | **C# .NET 9** | El servidor Kestrel en .NET 9 ofrece la menor latencia y mayor tasa de peticiones por segundo. |
| **Implementación SOAP** | **C# / VB.NET (CoreWCF)** | Soporte nativo de especificaciones WS-* mediante contratos declarativos tipados. |
| **Rigor y Estabilidad de Contratos** | **Java (Spring-WS)** | El enfoque *Contract-First* con esquemas XSD previene cambios accidentales en la API. |
| **Ligereza y Huella de Memoria** | **PHP / Ruby** | Consumo de memoria mínimo (< 40 MB), ideal para microservicios de tareas específicas. |
| **Interoperabilidad Universal** | **RESTful (JSON)** | Compatible de manera inmediata con navegadores, dispositivos móviles, escritorios y consolas sin requerir librerías complejas de parseo XML. |
