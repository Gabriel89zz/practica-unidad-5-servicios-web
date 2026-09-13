#!/usr/bin/env python3
"""
EcoLogistics CLI - Herramienta de Terminal para DevOps y Auditoría
Consume servicios heterogéneos RESTful y SOAP (Ruby :8084 y VB.NET :8086).
"""

import sys
import os
import argparse
import json
import time
import base64
import urllib.request
import urllib.error
import xml.etree.ElementTree as ET

# Reconfigurar salida estándar para compatibilidad universal con Windows (evitar errores de codepage cp1252)
if sys.platform == "win32":
    try:
        sys.stdout.reconfigure(encoding='utf-8')
        os.system('') # Habilitar secuencias de escape ANSI en cmd de Windows
    except Exception:
        pass

# Constantes de Colores ANSI para la Terminal
class Color:
    HEADER = '\033[95m'
    BLUE = '\033[94m'
    CYAN = '\033[96m'
    GREEN = '\033[92m'
    YELLOW = '\033[93m'
    RED = '\033[91m'
    BOLD = '\033[1m'
    DIM = '\033[2m'
    RESET = '\033[0m'

AUTH_TOKEN = "Bearer test_token_2026"

def print_banner():
    print(f"{Color.CYAN}{Color.BOLD}" + "=" * 75)
    print("  [>] EcoLogistics CLI - Consola de Operaciones y Auditoria Distribuida")
    print("  Programacion en Ambiente Cliente-Servidor * Consumo REST & SOAP")
    print("=" * 75 + f"{Color.RESET}\n")

def get_base_url(host: str, port: int, path: str = "") -> str:
    host = host.strip().rstrip('/')
    if not host.startswith("http://") and not host.startswith("https://"):
        host = "http://" + host
    return f"{host}:{port}{path}"

def http_request(url: str, method: str = "GET", headers: dict = None, body: str = None):
    if headers is None:
        headers = {}
    
    data = body.encode('utf-8') if body else None
    req = urllib.request.Request(url, data=data, headers=headers, method=method)
    
    start_time = time.time()
    try:
        with urllib.request.urlopen(req, timeout=10) as response:
            latency = int((time.time() - start_time) * 1000)
            status = response.status
            content = response.read().decode('utf-8', errors='ignore')
            return status, content, latency, None
    except urllib.error.HTTPError as e:
        latency = int((time.time() - start_time) * 1000)
        content = e.read().decode('utf-8', errors='ignore')
        return e.code, content, latency, e
    except Exception as e:
        latency = int((time.time() - start_time) * 1000)
        return 0, str(e), latency, e

# --- 1. Operaciones Ruby (:8084) - Notificaciones ---

def cmd_listar_destinatarios(host: str):
    url = get_base_url(host, 8084, "/api/v1/destinatarios")
    print(f"{Color.BLUE}[*] Consultando destinatarios REST en Ruby (:8084): {url}...{Color.RESET}")
    
    status, content, latency, err = http_request(url, method="GET", headers={"Authorization": AUTH_TOKEN})
    if status == 200:
        data = json.loads(content)
        print(f"{Color.GREEN}[OK] HTTP 200 OK ({latency} ms) - {len(data)} Destinatarios Registrados:{Color.RESET}\n")
        print(f"{Color.BOLD}{'ID':<12} {'NOMBRE':<28} {'CANAL':<8} {'TELÉFONO':<16} {'EMAIL':<30}{Color.RESET}")
        print("-" * 95)
        for d in data:
            print(f"{d.get('id',''):<12} {d.get('nombre',''):<28} {d.get('canal_preferido',''):<8} {d.get('telefono',''):<16} {d.get('email',''):<30}")
        print("-" * 95)
    else:
        print(f"{Color.RED}[FAIL] Error HTTP {status} ({latency} ms): {content}{Color.RESET}")

def cmd_despachar_alerta_soap(host: str, canal="EMAIL", destinatario="alerta@ecommerce.com", mensaje="Alerta de infraestructura", prioridad="ALTA"):
    url = get_base_url(host, 8084, "/soap/notificaciones")
    soap_xml = f"""<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:not="http://ecommerce.com/notificaciones">
  <soapenv:Header/>
  <soapenv:Body>
    <not:DespacharAlertaCritica>
      <tipoAlerta>INCIDENTE_SISTEMA</tipoAlerta>
      <canal>{canal}</canal>
      <destinatario>{destinatario}</destinatario>
      <mensaje>{mensaje}</mensaje>
      <prioridad>{prioridad}</prioridad>
    </not:DespacharAlertaCritica>
  </soapenv:Body>
</soapenv:Envelope>"""

    print(f"{Color.HEADER}[*] Despachando Alerta Critica SOAP en Ruby (:8084): {url}...{Color.RESET}")
    status, content, latency, err = http_request(
        url,
        method="POST",
        headers={"Content-Type": "text/xml; charset=utf-8"},
        body=soap_xml
    )

    if status == 200:
        try:
            root = ET.fromstring(content)
            msg_id = root.find(".//mensajeId")
            id_val = msg_id.text if msg_id is not None else "MSG-2026-XXXX"
            print(f"{Color.GREEN}[OK] SOAP HTTP 200 OK ({latency} ms) - Alerta Despachada Exitosamente:{Color.RESET}")
            print(f"    - ID de Mensaje : {Color.BOLD}{id_val}{Color.RESET}")
            print(f"    - Canal / Envio : {canal} -> {destinatario}")
            print(f"    - Prioridad     : {prioridad}")
            print(f"    - Mensaje       : \"{mensaje}\"")
        except Exception:
            print(f"{Color.GREEN}[OK] SOAP HTTP 200 OK:\n{content}{Color.RESET}")
    else:
        print(f"{Color.RED}[FAIL] Error SOAP HTTP {status} ({latency} ms):\n{content}{Color.RESET}")

# --- 2. Operaciones VB.NET (:8086) - Auditoría ---

def cmd_listar_logs_vbnet(host: str):
    url = get_base_url(host, 8086, "/api/v1/logs")
    print(f"{Color.BLUE}[*] Consultando Bitacora de Auditoria REST en VB.NET (:8086): {url}...{Color.RESET}")
    
    status, content, latency, err = http_request(url, method="GET", headers={"Authorization": AUTH_TOKEN})
    if status == 200:
        data = json.loads(content)
        print(f"{Color.GREEN}[OK] HTTP 200 OK ({latency} ms) - {len(data)} Registros de Auditoria:{Color.RESET}\n")
        print(f"{Color.BOLD}{'EVENTO ID':<18} {'SERVICIO':<18} {'ACCION':<16} {'NIVEL':<8} {'USUARIO':<14} {'DETALLES':<30}{Color.RESET}")
        print("-" * 105)
        for log in data:
            print(f"{log.get('evento_id',''):<18} {log.get('servicio_origen',''):<18} {log.get('accion',''):<16} {log.get('nivel',''):<8} {log.get('usuario',''):<14} {log.get('detalles',''):<30}")
        print("-" * 105)
    else:
        print(f"{Color.RED}[FAIL] Error HTTP {status} ({latency} ms): {content}{Color.RESET}")

def cmd_registrar_evento_soap_vbnet(host: str, servicio="CLI_DEVOPS", accion="AUDIT_INSPECTION", usuario="admin_cli", detalles="Inspeccion automatizada de seguridad"):
    url = get_base_url(host, 8086, "/soap/AuditoriaService.svc")
    soap_action = "http://ecommerce.com/auditoria/IAuditoriaService/RegistrarEvento"
    soap_xml = f"""<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
  <s:Body>
    <RegistrarEvento xmlns="http://ecommerce.com/auditoria">
      <request xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        <ServicioOrigen>{servicio}</ServicioOrigen>
        <Accion>{accion}</Accion>
        <Usuario>{usuario}</Usuario>
        <Nivel>INFO</Nivel>
        <Detalles>{detalles}</Detalles>
        <UsuarioAuth>admin</UsuarioAuth>
        <PasswordAuth>admin_pass_2026</PasswordAuth>
      </request>
    </RegistrarEvento>
  </s:Body>
</s:Envelope>"""

    print(f"{Color.HEADER}[*] Registrando Evento SOAP en VB.NET CoreWCF (:8086): {url}...{Color.RESET}")
    status, content, latency, err = http_request(
        url,
        method="POST",
        headers={
            "Content-Type": "text/xml; charset=utf-8",
            "SOAPAction": soap_action
        },
        body=soap_xml
    )

    if status == 200:
        try:
            root = ET.fromstring(content)
            evento_id = root.find(".//{http://ecommerce.com/auditoria}EventoId")
            if evento_id is None:
                for elem in root.iter():
                    if 'EventoId' in elem.tag:
                        evento_id = elem
                        break
            
            id_val = evento_id.text if evento_id is not None else "LOG-2026-XXXX"
            print(f"{Color.GREEN}[OK] SOAP HTTP 200 OK ({latency} ms) - Evento Auditado en VB.NET:{Color.RESET}")
            print(f"    - ID de Evento  : {Color.BOLD}{id_val}{Color.RESET}")
            print(f"    - Servicio/Host : {servicio}")
            print(f"    - Accion        : {accion}")
            print(f"    - Detalles      : {detalles}")
        except Exception:
            print(f"{Color.GREEN}[OK] SOAP HTTP 200 OK:\n{content}{Color.RESET}")
    else:
        print(f"{Color.RED}[FAIL] Error SOAP HTTP {status} ({latency} ms):\n{content}{Color.RESET}")

# --- 3. Menú Interactivo ---

def run_interactive_menu(host: str):
    while True:
        print_banner()
        print(f"  Servidor Configurado: {Color.BOLD}{host}{Color.RESET}\n")
        print("  Seleccione una operación a ejecutar:")
        print("   1. [REST]  Listar Destinatarios de Notificaciones (Ruby :8084)")
        print("   2. [SOAP]  Despachar Alerta Crítica (Ruby :8084)")
        print("   3. [REST]  Consultar Bitácora de Auditoría del Sistema (VB.NET :8086)")
        print("   4. [SOAP]  Registrar Evento de Seguridad en Auditoría (VB.NET :8086)")
        print("   5. [CONFIG] Cambiar Host / IP del Servidor")
        print("   0. Salir\n")

        try:
            opcion = input(f"{Color.BOLD}Opción [0-5]: {Color.RESET}").strip()
        except (KeyboardInterrupt, EOFError):
            print("\nSaliendo...")
            break

        print()
        if opcion == "1":
            cmd_listar_destinatarios(host)
        elif opcion == "2":
            dest = input("Destinatario (default: soporte@ecommerce.com): ").strip() or "soporte@ecommerce.com"
            msg = input("Mensaje de alerta (default: Falla en enlace logístico): ").strip() or "Falla en enlace logístico"
            cmd_despachar_alerta_soap(host, destinatario=dest, mensaje=msg)
        elif opcion == "3":
            cmd_listar_logs_vbnet(host)
        elif opcion == "4":
            acc = input("Acción a auditar (default: ACCESS_AUDIT): ").strip() or "ACCESS_AUDIT"
            det = input("Detalles del evento: ").strip() or "Inicio de sesión de administrador remoto"
            cmd_registrar_evento_soap_vbnet(host, accion=acc, detalles=det)
        elif opcion == "5":
            new_host = input("Nueva dirección IP o Host (ej. http://192.168.1.50): ").strip()
            if new_host:
                host = new_host
                print(f"{Color.GREEN}[✓] Host actualizado a: {host}{Color.RESET}")
        elif opcion == "0":
            print("Sesión terminada.")
            break
        else:
            print(f"{Color.RED}[!] Opción no válida.{Color.RESET}")

        print()
        input(f"{Color.DIM}Presione Enter para continuar...{Color.RESET}")

def main():
    parser = argparse.ArgumentParser(description="EcoLogistics CLI - Cliente de Terminal para Microservicios REST & SOAP")
    parser.add_argument("--host", default="http://localhost", help="Dirección base o IP del servidor (default: http://localhost)")
    parser.add_argument("--service", choices=["destinatarios", "alerta", "logs", "evento"], help="Ejecutar una operación directa")
    parser.add_argument("--mensaje", default="Alerta generada desde CLI", help="Mensaje para el despacho de alertas")
    parser.add_argument("--accion", default="AUDIT_EVENT", help="Acción para el registro de auditoría")
    args = parser.parse_args()

    if args.service:
        print_banner()
        if args.service == "destinatarios":
            cmd_listar_destinatarios(args.host)
        elif args.service == "alerta":
            cmd_despachar_alerta_soap(args.host, mensaje=args.mensaje)
        elif args.service == "logs":
            cmd_listar_logs_vbnet(args.host)
        elif args.service == "evento":
            cmd_registrar_evento_soap_vbnet(args.host, accion=args.accion)
    else:
        run_interactive_menu(args.host)

if __name__ == "__main__":
    main()
