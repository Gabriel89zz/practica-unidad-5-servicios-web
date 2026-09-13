# Cliente de Escritorio de Facturación y Logística (EcoLogistics Desktop Suite)

**Plataforma:** Aplicación de Escritorio Nativa (.NET 9 / C# Windows Forms)  
**Materia:** Programación en Ambiente Cliente-Servidor (7.° Semestre)  
**Propósito:** Demostrar el rol de *cliente pesado / administrativo* consumiendo servicios heterogéneos mediante **SOAP (CoreWCF y WSDL)** y **REST (JSON)** con soporte de red local y remota.

---

## 1. Microservicios Consumidos por la Aplicación de Escritorio

| Microservicio | Puerto / Plataforma | Protocolo | Operación Ejecutada | Propósito de Negocio |
| :--- | :---: | :---: | :--- | :--- |
| **C# .NET 9 / CoreWCF** | `:8085` | **SOAP** | `POST /soap/FacturacionService.svc` (`TimbrarComprobante`) | Emisión y timbrado de comprobante fiscal digital SAT con generación de UUID y sello digital. |
| **C# .NET 9 / Web API** | `:8085` | **REST** | `GET /api/v1/facturas` | Carga del historial de comprobantes timbrados en una tabla interactiva (`DataGridView`). |
| **Python 3.12 / FastAPI** | `:8082` | **SOAP** | `POST /soap/envios` (`CalcularTarifa`) | Cotizador de costos y tiempos de entrega según origen, destino y peso en kilogramos. |
| **Python 3.12 / FastAPI** | `:8082` | **REST** | `GET /api/v1/envios/{guia}` | Rastreo en tiempo real del historial de eventos logísticos de un paquete. |

---

## 2. Características del Cliente

1. **Selector de Servidor Base:**
   * Campo editable en la parte superior para apuntar a `http://localhost` o a la IP remota del servidor Debian (ej. `http://192.168.1.50`).
   * Botón **"Probar Conexión"** para validar que los puertos 8085 y 8082 estén alcanzables.
2. **Pestaña 1: Facturación y Timbrado SAT:**
   * Cálculo automático de IVA (16%) y Total a partir del Subtotal.
   * Envío de sobre SOAP XML con credenciales `admin` / `admin_pass_2026`.
   * Visualización del UUID fiscal y sellos criptográficos devueltos por el SAT simulado.
   * Listado en vivo de todas las facturas en base de datos en memoria mediante REST.
3. **Pestaña 2: Logística y Paquetería:**
   * Formulario de cotización SOAP para cálculo de tarifas según kilometraje y peso.
   * Rastreo REST de números de guía (ej. `GUIA-2026-9081`) con desglose de eventos.
4. **Pestaña 3: Monitor de Red Distribuida:**
   * Registro en tiempo real de cada sobre SOAP o JSON transmitido, cabeceras HTTP y latencias.

---

## 3. ¿Cómo Ejecutar la Aplicación de Escritorio?

Desde la terminal en Windows (PowerShell):

```powershell
# 1. Navegar a la carpeta del cliente desktop
cd clients/desktop-app

# 2. Ejecutar la aplicación
dotnet run
```
O directamente haciendo doble clic en el ejecutable generado en:
`clients/desktop-app/bin/Debug/net9.0-windows/DesktopApp.exe`
