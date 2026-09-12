# Referencia Completa de APIs RESTful
**Sistema Distribuido de Comercio Electrónico y Logística**  
**Autenticación Requerida:** `Authorization: Bearer test_token_2026` o `X-API-Key: secret_key_cs`

---

## Índice de Microservicios

1. [Módulo 1: Catálogo e Inventario (Java - Puerto 8081)](#1-módulo-catálogo-e-inventario-java---8081)
2. [Módulo 2: Envíos y Cálculo de Rutas (Python - Puerto 8082)](#2-módulo-envíos-y-cálculo-de-rutas-python---8082)
3. [Módulo 3: Gestión de Órdenes de Compra (PHP - Puerto 8083)](#3-módulo-gestión-de-órdenes-de-compra-php---8083)
4. [Módulo 4: Notificaciones y Webhooks (Ruby - Puerto 8084)](#4-módulo-notificaciones-y-webhooks-ruby---8084)
5. [Módulo 5: Facturación y Timbrado (C# .NET - Puerto 8085)](#5-módulo-facturación-y-timbrado-c-net---8085)
6. [Módulo 6: Auditoría y Bitácoras (VB.NET - Puerto 8086)](#6-módulo-auditoría-y-bitácoras-vbnet---8086)

---

## 1. Módulo: Catálogo e Inventario (Java - :8081)
**Swagger UI:** `http://localhost:8081/swagger-ui.html`  
**OpenAPI JSON:** `http://localhost:8081/v3/api-docs`

### 1.1 Listar Productos
- **Método:** `GET`
- **URL:** `http://localhost:8081/api/v1/productos`
- **Parámetros Opcionales:**
  - `categoria`: Filtrar por categoría (ej. `Computadoras`, `Periféricos`).
  - `busqueda`: Término de búsqueda textual por SKU o nombre.
- **Ejemplo cURL:**
  ```bash
  curl -X GET "http://localhost:8081/api/v1/productos?categoria=Periféricos" \
       -H "Authorization: Bearer test_token_2026"
  ```
- **Respuesta 200 OK:**
  ```json
  [
    {
      "id": 2,
      "sku": "MOU-002",
      "nombre": "Mouse Inalámbrico Logitech MX Master 3S",
      "descripcion": "Sensor óptico 8000 DPI, silencioso",
      "precio": 99.99,
      "stock": 45,
      "categoria": "Periféricos",
      "activo": true
    }
  ]
  ```

### 1.2 Obtener Producto por ID
- **Método:** `GET`
- **URL:** `http://localhost:8081/api/v1/productos/{id}`
- **Ejemplo cURL:**
  ```bash
  curl -X GET "http://localhost:8081/api/v1/productos/1" \
       -H "Authorization: Bearer test_token_2026"
  ```

### 1.3 Crear Producto
- **Método:** `POST`
- **URL:** `http://localhost:8081/api/v1/productos`
- **Payload:**
  ```json
  {
    "sku": "TAB-006",
    "nombre": "iPad Air M2 11 pulgadas",
    "descripcion": "Chip M2, 128GB Wi-Fi Gris Espacial",
    "precio": 599.00,
    "stock": 20,
    "categoria": "Tablets"
  }
  ```
- **Ejemplo cURL:**
  ```bash
  curl -X POST "http://localhost:8081/api/v1/productos" \
       -H "Content-Type: application/json" \
       -H "Authorization: Bearer test_token_2026" \
       -d '{"sku":"TAB-006","nombre":"iPad Air M2","descripcion":"Chip M2","precio":599.00,"stock":20,"categoria":"Tablets"}'
  ```

### 1.4 Eliminar Producto
- **Método:** `DELETE`
- **URL:** `http://localhost:8081/api/v1/productos/{id}`

---

## 2. Módulo: Envíos y Cálculo de Rutas (Python - :8082)
**Swagger UI:** `http://localhost:8082/docs`  
**ReDoc:** `http://localhost:8082/redoc`

### 2.1 Listar Envíos
- **Método:** `GET`
- **URL:** `http://localhost:8082/api/v1/envios`
- **Ejemplo cURL:**
  ```bash
  curl -X GET "http://localhost:8082/api/v1/envios" \
       -H "X-API-Key: secret_key_cs"
  ```

### 2.2 Registrar Nuevo Envío
- **Método:** `POST`
- **URL:** `http://localhost:8082/api/v1/envios`
- **Payload:**
  ```json
  {
    "remitente_nombre": "Almacén Central Tech",
    "remitente_direccion": "Av. Insurgentes Sur 1602, CDMX",
    "destinatario_nombre": "María Morales",
    "destinatario_direccion": "Av. Hidalgo 100, Monterrey, NL",
    "peso_kg": 4.5,
    "tipo_servicio": "EXPRESS",
    "descripcion_contenido": "Equipos de cómputo frágiles"
  }
  ```

### 2.3 Rastrear Paquete por Guía
- **Método:** `GET`
- **URL:** `http://localhost:8082/api/v1/envios/{numero_guia}`
- **Ejemplo cURL:**
  ```bash
  curl -X GET "http://localhost:8082/api/v1/envios/GUIA-2026-9081" \
       -H "Authorization: Bearer test_token_2026"
  ```

### 2.4 Actualizar Estado Logístico
- **Método:** `PUT`
- **URL:** `http://localhost:8082/api/v1/envios/{numero_guia}/estado`
- **Payload:**
  ```json
  {
    "nuevo_estado": "EN_DISTRIBUCION",
    "ubicacion_actual": "Unidad de Reparto Monterrey Norte",
    "comentario": "El paquete salió a ruta final con el repartidor"
  }
  ```

---

## 3. Módulo: Gestión de Órdenes de Compra (PHP - :8083)
**Swagger UI:** `http://localhost:8083/docs`  
**OpenAPI JSON:** `http://localhost:8083/openapi.json`

### 3.1 Listar Órdenes
- **Método:** `GET`
- **URL:** `http://localhost:8083/api/v1/ordenes`
- **Ejemplo cURL:**
  ```bash
  curl -X GET "http://localhost:8083/api/v1/ordenes" \
       -H "Authorization: Bearer test_token_2026"
  ```

### 3.2 Crear Nueva Orden
- **Método:** `POST`
- **URL:** `http://localhost:8083/api/v1/ordenes`
- **Payload:**
  ```json
  {
    "cliente_id": "CLI-005",
    "cliente_nombre": "Fernando Castillo",
    "cliente_email": "fernando.castillo@example.com",
    "direccion_envio": "Paseo de la Reforma 505, CDMX",
    "metodo_pago": "TARJETA_DEBITO",
    "items": [
      {
        "sku": "LAP-001",
        "nombre": "Laptop Dell XPS 15",
        "cantidad": 1,
        "precio_unitario": 1899.99
      },
      {
        "sku": "MOU-002",
        "nombre": "Mouse Inalámbrico Logitech MX Master 3S",
        "cantidad": 2,
        "precio_unitario": 99.99
      }
    ]
  }
  ```

### 3.3 Consultar Detalle de Orden
- **Método:** `GET`
- **URL:** `http://localhost:8083/api/v1/ordenes/{id}`

### 3.4 Cancelar Orden
- **Método:** `DELETE`
- **URL:** `http://localhost:8083/api/v1/ordenes/{id}`

---

## 4. Módulo: Notificaciones y Webhooks (Ruby - :8084)
**Swagger UI:** `http://localhost:8084/docs`  
**OpenAPI JSON:** `http://localhost:8084/openapi.json`

### 4.1 Listar Destinatarios
- **Método:** `GET`
- **URL:** `http://localhost:8084/api/v1/destinatarios`
- **Ejemplo cURL:**
  ```bash
  curl -X GET "http://localhost:8084/api/v1/destinatarios" \
       -H "Authorization: Bearer test_token_2026"
  ```

### 4.2 Registrar Destinatario
- **Método:** `POST`
- **URL:** `http://localhost:8084/api/v1/destinatarios`
- **Payload:**
  ```json
  {
    "nombre": "Ing. Carlos Santana",
    "email": "carlos.santana@empresa.com",
    "telefono": "+525544332211",
    "canal_preferido": "EMAIL"
  }
  ```

### 4.3 Listar Plantillas
- **Método:** `GET`
- **URL:** `http://localhost:8084/api/v1/plantillas`

### 4.4 Listar Historial de Alertas
- **Método:** `GET`
- **URL:** `http://localhost:8084/api/v1/alertas`

---

## 5. Módulo: Facturación y Timbrado (C# .NET - :8085)
**Swagger UI:** `http://localhost:8085/swagger`

### 5.1 Listar Facturas Emitidas
- **Método:** `GET`
- **URL:** `http://localhost:8085/api/v1/facturas`
- **Filtros Opcionales:** `?rfc=GOLF850315ABC` o `?estatus=VIGENTE`
- **Ejemplo cURL:**
  ```bash
  curl -X GET "http://localhost:8085/api/v1/facturas" \
       -H "Authorization: Bearer test_token_2026"
  ```

### 5.2 Obtener Factura por Folio
- **Método:** `GET`
- **URL:** `http://localhost:8085/api/v1/facturas/{folio}`
- **Ejemplo:** `http://localhost:8085/api/v1/facturas/FAC-2026-001`

### 5.3 Crear Factura
- **Método:** `POST`
- **URL:** `http://localhost:8085/api/v1/facturas`
- **Payload:**
  ```json
  {
    "rfcCliente": "XAXX010101000",
    "razonSocialCliente": "Público en General",
    "subtotal": 1000.00,
    "impuestos": 160.00,
    "total": 1160.00,
    "concepto": "Servicios de logística y venta de equipo",
    "metodoPago": "PUE"
  }
  ```

---

## 6. Módulo: Auditoría y Bitácoras (VB.NET - :8086)
**Swagger UI:** `http://localhost:8086/swagger`

### 6.1 Listar Logs de Auditoría
- **Método:** `GET`
- **URL:** `http://localhost:8086/api/v1/logs`
- **Filtros Opcionales:** `?servicio=MODULO-INVENTARIO` o `?nivel=INFO`
- **Ejemplo cURL:**
  ```bash
  curl -X GET "http://localhost:8086/api/v1/logs" \
       -H "Authorization: Bearer test_token_2026"
  ```

### 6.2 Registrar Evento Directo
- **Método:** `POST`
- **URL:** `http://localhost:8086/api/v1/logs`
- **Payload:**
  ```json
  {
    "servicioOrigen": "FRONTEND_ADMIN",
    "accion": "LOGIN_USUARIO",
    "usuario": "admin_central",
    "nivel": "INFO",
    "detalles": "Acceso concedido al panel administrativo desde IP 192.168.1.50"
  }
  ```

### 6.3 Purgar Bitácora
- **Método:** `DELETE`
- **URL:** `http://localhost:8086/api/v1/logs`
- **Ejemplo cURL:**
  ```bash
  curl -X DELETE "http://localhost:8086/api/v1/logs" \
       -H "Authorization: Bearer test_token_2026"
  ```
