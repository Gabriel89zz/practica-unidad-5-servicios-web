# Cliente de Terminal / CLI de Auditoría y Notificaciones (EcoLogistics CLI)

**Plataforma:** Herramienta de Línea de Comandos (Python 3.12 / Standard Library)  
**Materia:** Programación en Ambiente Cliente-Servidor (7.° Semestre)  
**Propósito:** Demostrar el rol de *cliente DevOps / SysAdmin* para tareas de monitoreo, consulta de bitácoras de auditoría y despacho de alertas críticas consumiendo servicios **REST (JSON)** y **SOAP (XML/WSDL)** sin necesidad de interfaces gráficas pesadas.

---

## 1. Microservicios Consumidos por la Herramienta CLI

| Microservicio | Puerto / Plataforma | Protocolo | Operación Ejecutada | Propósito de Negocio |
| :--- | :---: | :---: | :--- | :--- |
| **Ruby 3.2+ / Sinatra** | `:8084` | **REST** | `GET /api/v1/destinatarios` | Consulta del directorio de contactos para notificación multicanal. |
| **Ruby 3.2+ / Nokogiri** | `:8084` | **SOAP** | `POST /soap/notificaciones` (`DespacharAlertaCritica`) | Emisión urgente de alertas críticas hacia el motor de notificaciones. |
| **VB.NET .NET 9 / Web API** | `:8086` | **REST** | `GET /api/v1/logs` | Inspección de la bitácora centralizada de eventos del sistema. |
| **VB.NET .NET 9 / CoreWCF** | `:8086` | **SOAP** | `POST /soap/AuditoriaService.svc` (`RegistrarEvento`) | Registro transaccional de incidentes y accesos en la bitácora atómica. |

---

## 2. Modos de Ejecución

Esta herramienta no requiere instalación de dependencias externas (utiliza únicamente la librería estándar de Python).

### Modo 1: Menú Interactivo de Terminal
Ideal para demostraciones interactivas paso a paso ante el profesor:

```bash
python clients/cli-tool/cli.py
```
Desplegará un menú interactivo con colores ANSI para seleccionar operaciones, ingresar destinatarios o cambiar la dirección del servidor Debian en caliente.

### Modo 2: Comandos Directos por Argumentos
Ideal para scripts automatizados o tareas programadas (*Cron/Batch*):

```bash
# 1. Consultar destinatarios en Ruby REST (puerto 8084)
python clients/cli-tool/cli.py --host http://localhost --service destinatarios

# 2. Despachar alerta crítica vía SOAP en Ruby (puerto 8084)
python clients/cli-tool/cli.py --host http://localhost --service alerta --mensaje "Falla de red detectada en nodo 3"

# 3. Listar bitácora de auditoría en VB.NET REST (puerto 8086)
python clients/cli-tool/cli.py --host http://localhost --service logs

# 4. Registrar evento de seguridad en VB.NET CoreWCF SOAP (puerto 8086)
python clients/cli-tool/cli.py --host http://localhost --service evento --accion "SEGURIDAD_ACCESO"
```
*(Nota: Para apuntar al servidor Debian remoto, solo cambia `--host http://localhost` por `--host http://IP_DE_TU_DEBIAN`).*
