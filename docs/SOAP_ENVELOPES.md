# Catálogo de Contratos y Envoltorios SOAP (XML)
**Práctica de Programación en Ambiente Cliente-Servidor**  
**Credenciales Autorizadas:** Usuario: `admin` | Contraseña: `admin_pass_2026`  
**Header HTTP Basic Auth:** `Authorization: Basic YWRtaW46YWRtaW5fcGFzc18yMDI2`

---

## 1. Módulo: Catálogo e Inventario (Java - :8081)
- **WSDL:** `http://localhost:8081/ws/inventario.wsdl`
- **Endpoint:** `POST http://localhost:8081/ws`

### Operación: `ConsultarStockRequest`
#### Envoltorio de Solicitud (Request):
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:inv="http://ecommerce.com/inventario">
   <soapenv:Header>
      <inv:AuthHeader>
         <inv:Username>admin</inv:Username>
         <inv:Password>admin_pass_2026</inv:Password>
      </inv:AuthHeader>
   </soapenv:Header>
   <soapenv:Body>
      <inv:ConsultarStockRequest>
         <inv:sku>LAP-001</inv:sku>
      </inv:ConsultarStockRequest>
   </soapenv:Body>
</soapenv:Envelope>
```

#### Envoltorio de Respuesta (Response):
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
   <soapenv:Body>
      <tns:ConsultarStockResponse xmlns:tns="http://ecommerce.com/inventario">
         <tns:sku>LAP-001</tns:sku>
         <tns:nombre>Laptop Dell XPS 15</tns:nombre>
         <tns:stockDisponible>15</tns:stockDisponible>
         <tns:disponible>true</tns:disponible>
         <tns:precioUnitario>1899.99</tns:precioUnitario>
         <tns:categoria>Computadoras</tns:categoria>
      </tns:ConsultarStockResponse>
   </soapenv:Body>
</soapenv:Envelope>
```

#### Comando cURL:
```bash
curl -X POST "http://localhost:8081/ws" \
     -H "Content-Type: text/xml; charset=utf-8" \
     -d '<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:inv="http://ecommerce.com/inventario"><soapenv:Body><inv:ConsultarStockRequest><inv:sku>LAP-001</inv:sku></inv:ConsultarStockRequest></soapenv:Body></soapenv:Envelope>'
```

---

## 2. Módulo: Envíos y Rutas (Python Spyne - :8082)
- **WSDL:** `http://localhost:8082/soap/envios?wsdl`
- **Endpoint:** `POST http://localhost:8082/soap/envios`

### Operación: `CalcularTarifa`
#### Envoltorio de Solicitud (Request):
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:env="http://ecommerce.com/envios">
   <soapenv:Header/>
   <soapenv:Body>
      <env:CalcularTarifa>
         <env:origen>CDMX Centro</env:origen>
         <env:destino>Guadalajara Jalisco</env:destino>
         <env:peso_kg>3.5</env:peso_kg>
         <env:tipo_servicio>EXPRESS</env:tipo_servicio>
      </env:CalcularTarifa>
   </soapenv:Body>
</soapenv:Envelope>
```

#### Envoltorio de Respuesta (Response):
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tns="http://ecommerce.com/envios">
   <soapenv:Body>
      <tns:CalcularTarifaResponse>
         <tns:CalcularTarifaResult>
            <tns:origen>CDMX Centro</tns:origen>
            <tns:destino>Guadalajara Jalisco</tns:destino>
            <tns:peso_kg>3.5</tns:peso_kg>
            <tns:tipo_servicio>EXPRESS</tns:tipo_servicio>
            <tns:costo_envio>549.0</tns:costo_envio>
            <tns:tiempo_estimado_dias>1</tns:tiempo_estimado_dias>
            <tns:moneda>MXN</tns:moneda>
            <tns:distancia_aprox_km>550</tns:distancia_aprox_km>
         </tns:CalcularTarifaResult>
      </tns:CalcularTarifaResponse>
   </soapenv:Body>
</soapenv:Envelope>
```

#### Comando cURL:
```bash
curl -X POST "http://localhost:8082/soap/envios" \
     -H "Content-Type: text/xml; charset=utf-8" \
     -H "Authorization: Basic YWRtaW46YWRtaW5fcGFzc18yMDI2" \
     -d '<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:env="http://ecommerce.com/envios"><soapenv:Body><env:CalcularTarifa><env:origen>CDMX</env:origen><env:destino>Guadalajara</env:destino><env:peso_kg>3.5</env:peso_kg><env:tipo_servicio>EXPRESS</env:tipo_servicio></env:CalcularTarifa></soapenv:Body></soapenv:Envelope>'
```

---

## 3. Módulo: Gestión de Órdenes (PHP SoapServer - :8083)
- **WSDL:** `http://localhost:8083/soap/ordenes.wsdl`
- **Endpoint:** `POST http://localhost:8083/soap/ordenes`

### Operación: `ValidarEstadoOrden`
#### Envoltorio de Solicitud:
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:ord="http://ecommerce.com/ordenes">
   <soapenv:Header/>
   <soapenv:Body>
      <ord:ValidarEstadoOrden>
         <ordenId>ORD-2026-101</ordenId>
      </ord:ValidarEstadoOrden>
   </soapenv:Body>
</soapenv:Envelope>
```

#### Envoltorio de Respuesta:
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tns="http://ecommerce.com/ordenes">
  <soap:Body>
    <tns:ValidarEstadoOrdenResponse>
      <resultado>
        <id>ORD-2026-101</id>
        <clienteId>CLI-001</clienteId>
        <clienteNombre>Laura Gómez Flores</clienteNombre>
        <estado>PAGADA</estado>
        <total>2203.98</total>
        <pagado>true</pagado>
        <fechaCreacion>2026-09-11T19:24:00+00:00</fechaCreacion>
        <valido>true</valido>
        <mensaje>Orden encontrada y validada con estado: PAGADA</mensaje>
      </resultado>
    </tns:ValidarEstadoOrdenResponse>
  </soap:Body>
</soap:Envelope>
```

---

## 4. Módulo: Notificaciones y Webhooks (Ruby - :8084)
- **WSDL:** `http://localhost:8084/soap/notificaciones/wsdl`
- **Endpoint:** `POST http://localhost:8084/soap/notificaciones`

### Operación: `DespacharAlertaCritica`
#### Envoltorio de Solicitud:
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:not="http://ecommerce.com/notificaciones">
   <soapenv:Header/>
   <soapenv:Body>
      <not:DespacharAlertaCritica>
         <tipoAlerta>FALLA_PAGO</tipoAlerta>
         <canal>EMAIL</canal>
         <destinatario>laura.gomez@example.com</destinatario>
         <mensaje>Rechazo en la tarjeta de crédito para la orden 102</mensaje>
         <prioridad>ALTA</prioridad>
      </not:DespacharAlertaCritica>
   </soapenv:Body>
</soapenv:Envelope>
```

#### Envoltorio de Respuesta:
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tns="http://ecommerce.com/notificaciones">
  <soapenv:Body>
    <tns:DespacharAlertaCriticaResponse>
      <resultado>
        <mensajeId>MSG-2026-A19F</mensajeId>
        <tipoAlerta>FALLA_PAGO</tipoAlerta>
        <canal>EMAIL</canal>
        <destinatario>laura.gomez@example.com</destinatario>
        <entregado>true</entregado>
        <timestamp>2026-09-11T19:30:15Z</timestamp>
        <estado>DESPACHADO_EXITOSAMENTE</estado>
        <detalle>Alerta de prioridad ALTA encolada y entregada al canal EMAIL</detalle>
      </resultado>
    </tns:DespacharAlertaCriticaResponse>
  </soapenv:Body>
</soapenv:Envelope>
```

---

## 5. Módulo: Facturación y Timbrado (C# CoreWCF - :8085)
- **WSDL:** `http://localhost:8085/soap/FacturacionService.svc?wsdl`
- **Endpoint:** `POST http://localhost:8085/soap/FacturacionService.svc`

### Operación: `TimbrarComprobante`
#### Envoltorio de Solicitud:
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:fac="http://ecommerce.com/facturacion">
   <soapenv:Header/>
   <soapenv:Body>
      <fac:TimbrarComprobante>
         <fac:request>
            <fac:RfcEmisor>ECO20260101ECO</fac:RfcEmisor>
            <fac:RfcReceptor>GOLF850315ABC</fac:RfcReceptor>
            <fac:Subtotal>1899.99</fac:Subtotal>
            <fac:Iva>303.99</fac:Iva>
            <fac:Total>2203.98</fac:Total>
            <fac:Concepto>Adquisicion de equipo de computo y accesorios</fac:Concepto>
            <fac:FormaPago>03</fac:FormaPago>
            <fac:UsuarioAuth>admin</fac:UsuarioAuth>
            <fac:PasswordAuth>admin_pass_2026</fac:PasswordAuth>
         </fac:request>
      </fac:TimbrarComprobante>
   </soapenv:Body>
</soapenv:Envelope>
```

#### Envoltorio de Respuesta:
```xml
<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
   <s:Body>
      <TimbrarComprobanteResponse xmlns="http://ecommerce.com/facturacion">
         <TimbrarComprobanteResult xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
            <CodigoRespuesta>SAT-000-OK</CodigoRespuesta>
            <Estatus>TIMBRADO</Estatus>
            <FechaTimbrado>2026-09-11T19:30:00.0000000Z</FechaTimbrado>
            <FolioFiscalUUID>7D9C4A21-5E3B-4C8D-91E0-F2A3B4C5D6E7</FolioFiscalUUID>
            <Mensaje>Comprobante fiscal timbrado con éxito bajo folio 7D9C4A21-...</Mensaje>
            <NoCertificadoSAT>30001000000500003416</NoCertificadoSAT>
            <RfcEmisor>ECO20260101ECO</RfcEmisor>
            <RfcReceptor>GOLF850315ABC</RfcReceptor>
            <SelloDigitalEmisor>A1B2C3D4E5...</SelloDigitalEmisor>
            <SelloDigitalSAT>F1E2D3C4B5...</SelloDigitalSAT>
            <Total>2203.98</Total>
         </TimbrarComprobanteResult>
      </TimbrarComprobanteResponse>
   </s:Body>
</s:Envelope>
```

---

## 6. Módulo: Auditoría y Bitácoras (VB.NET CoreWCF - :8086)
- **WSDL:** `http://localhost:8086/soap/AuditoriaService.svc?wsdl`
- **Endpoint:** `POST http://localhost:8086/soap/AuditoriaService.svc`

### Operación: `RegistrarEvento`
#### Envoltorio de Solicitud:
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:aud="http://ecommerce.com/auditoria">
   <soapenv:Header/>
   <soapenv:Body>
      <aud:RegistrarEvento>
         <aud:request>
            <aud:ServicioOrigen>MODULO-FACTURACION</aud:ServicioOrigen>
            <aud:Accion>TIMBRADO_COMPROBANTE</aud:Accion>
            <aud:Usuario>gateway_sat</aud:Usuario>
            <aud:Nivel>INFO</aud:Nivel>
            <aud:Detalles>Timbrado generado con UUID 7D9C4A21-... por total $2,203.98</aud:Detalles>
            <aud:UsuarioAuth>admin</aud:UsuarioAuth>
            <aud:PasswordAuth>admin_pass_2026</aud:PasswordAuth>
         </aud:request>
      </aud:RegistrarEvento>
   </soapenv:Body>
</soapenv:Envelope>
```

#### Envoltorio de Respuesta:
```xml
<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
   <s:Body>
      <RegistrarEventoResponse xmlns="http://ecommerce.com/auditoria">
         <RegistrarEventoResult xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
            <EventoId>LOG-2026-F981A0</EventoId>
            <Mensaje>Evento registrado atómicamente en la bitácora del sistema.</Mensaje>
            <RegistradoExitosamente>true</RegistradoExitosamente>
            <Timestamp>2026-09-11T19:30:15.1234567Z</Timestamp>
         </RegistrarEventoResult>
      </RegistrarEventoResponse>
   </s:Body>
</s:Envelope>
```

---

## 7. Manejo de Errores SOAP (Estructura de SOAP Fault)

Cuando un cliente envía credenciales incorrectas o datos no válidos, el servicio responde con código HTTP `500 Internal Server Error` y un sobre `SOAP-ENV:Fault`:

```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
   <soapenv:Body>
      <soapenv:Fault>
         <faultcode>soapenv:Client.AuthenticationFailed</faultcode>
         <faultstring>Credenciales SOAP inválidas o no autorizadas.</faultstring>
         <detail>
            <errorCode>AUTH_001</errorCode>
            <timestamp>2026-09-11T19:30:00Z</timestamp>
         </detail>
      </soapenv:Fault>
   </soapenv:Body>
</soapenv:Envelope>
```
