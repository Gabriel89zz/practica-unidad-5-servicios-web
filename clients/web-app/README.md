# Cliente Web de Comercio Electrónico Distribuido (EcoLogistics Store)

**Plataforma:** Web Application (HTML5, CSS3 Glassmorphism, JavaScript ES6+ asíncrono)  
**Materia:** Programación en Ambiente Cliente-Servidor (7.° Semestre)  
**Propósito:** Demostrar el consumo síncrono y asíncrono de microservicios heterogéneos mediante protocolos **RESTful (JSON)** y **SOAP (XML/WSDL)** desde una aplicación cliente web.

---

## 1. Microservicios Consumidos por la Aplicación Web

| Microservicio | Plataforma / Puerto | Protocolo | Operación Ejecutada | Propósito de Negocio |
| :--- | :---: | :---: | :--- | :--- |
| **Java 21 / Spring Boot 3** | `:8081` | **REST** | `GET /api/v1/productos` | Obtiene el catálogo de productos con precios, stock y categorías. |
| **Java 21 / Spring-WS** | `:8081` | **SOAP** | `POST /ws` (`ConsultarStockRequest`) | Valida el inventario físico en tiempo real directamente sobre el contrato WSDL. |
| **PHP 8.2 / Slim 4** | `:8083` | **REST** | `POST /api/v1/ordenes` | Genera una orden de compra en SQLite con cálculo de impuestos y número de folio. |
| **PHP 8.2 / ext-soap** | `:8083` | **SOAP** | `POST /soap/ordenes` (`ValidarEstadoOrden`) | Verifica la validez y estado de la orden recién creada mediante contrato WSDL RPC/literal. |
| **Python 3.12 / FastAPI** | `:8082` | **SOAP** | `POST /soap/envios` (`CalcularTarifa`) | Cotiza el flete logístico y tiempo de entrega según origen, destino, peso y modalidad. |

---

## 2. Características Principales

1. **Configuración Dinámica de Servidor:**
   * Barra superior con selector de host/IP para alternar entre `http://localhost` (pruebas locales) o la IP remota del servidor Debian (ej. `http://192.168.1.50`).
   * Botón **"Probar Conexión"** con indicador visual de estado en tiempo real.
2. **Inspector de Protocolos en Red en Vivo:**
   * Panel inferior desplegable que captura cada solicitud y respuesta.
   * Muestra encabezados HTTP (`Authorization: Bearer test_token_2026`, `SOAPAction`), cuerpos de petición en crudo (JSON o XML), códigos de estado HTTP y tiempos de latencia en milisegundos.
3. **Flujo Transaccional Completo:**
   * Catálogo con filtros por categoría y búsqueda textual.
   * Carrito de compras reactivo con desglose de Subtotal, IVA (16%) y Total.
   * Generación y confirmación de pedidos con enlace directo a validación SOAP.
   * Cotizador logístico integrado con despacho de sobres XML hacia Python.

---

## 3. ¿Cómo Ejecutar la Aplicación Web?

No requiere compilación ni servidores intermedios de NodeJS:

### Opción A: Abrir directamente en el navegador
Haz doble clic sobre el archivo `index.html` en el explorador de archivos de Windows o ábrelo directamente en tu navegador (Chrome, Edge, Firefox):
```text
file:///c:/Users/elcom/OneDrive/Desktop/UNI/Semestre 7/Programacion en ambiente cliente-servidor/practica-unidad-5/clients/web-app/index.html
```

### Opción B: Con cualquier servidor estático local (opcional)
Si prefieres servirlo mediante un servidor HTTP local:
```bash
# Con Python
python -m http.server 3000 --directory clients/web-app

# Y abrir en: http://localhost:3000
```
