#!/usr/bin/env bash
# ==============================================================================
# Script Principal: run.sh
# Menú interactivo unificado para orquestar servicios, pruebas y clientes.
# ==============================================================================

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$DIR" || exit 1

# Colores ANSI
CYAN="\033[0;36m"
GREEN="\033[0;32m"
YELLOW="\033[1;33m"
BLUE="\033[0;34m"
MAGENTA="\033[0;35m"
BOLD="\033[1m"
RESET="\033[0m"

# Asegurar permisos de ejecución en scripts auxiliares
chmod +x "$DIR/scripts/"*.sh 2>/dev/null

show_menu() {
    clear 2>/dev/null || echo ""
    echo -e "${BOLD}${CYAN}======================================================================${RESET}"
    echo -e "${BOLD}${CYAN}   SISTEMA DISTRIBUIDO DE COMERCIO ELECTRONICO Y LOGISTICA${RESET}"
    echo -e "${CYAN}   Practica Unidad 5: Programacion en Ambiente Cliente-Servidor${RESET}"
    echo -e "${BOLD}${CYAN}======================================================================${RESET}"
    echo -e "${BOLD}   [1]${RESET} Iniciar Servicios Backend (Docker Compose)"
    echo -e "${BOLD}   [2]${RESET} Detener Servicios Backend (Docker Compose Down)"
    echo -e "${BOLD}   [3]${RESET} Verificar Salud y Estado de Servicios (Puertos 8081-8086)"
    echo -e "${BOLD}   [4]${RESET} Ejecutar Suite de Pruebas Automatizadas (REST & SOAP)"
    echo -e "${BOLD}${MAGENTA}   --- APLICACIONES CLIENTE ---${RESET}"
    echo -e "${BOLD}   [5]${RESET} Lanzar Cliente Web SPA (Tienda en Linea - HTML5/CSS/JS)"
    echo -e "${BOLD}   [6]${RESET} Lanzar Cliente de Terminal / CLI (Python 3 - Ruby & VB.NET)"
    echo -e "${BOLD}   [7]${RESET} Lanzar Cliente de Escritorio (C# WinForms - C# & Python)"
    echo -e "${BOLD}${CYAN}   ------------------------------------------------------------------${RESET}"
    echo -e "${BOLD}   [0]${RESET} Salir"
    echo -e "${BOLD}${CYAN}======================================================================${RESET}"
    echo -n "Seleccione una opción [0-7]: "
}

ask_host() {
    read -r -p "Ingrese la URL del host [Enter para http://localhost]: " TARGET_HOST
    TARGET_HOST="${TARGET_HOST:-http://localhost}"
    TARGET_HOST="${TARGET_HOST%/}"
}

while true; do
    show_menu
    read -r OPTION
    echo ""

    case "$OPTION" in
        1)
            bash "$DIR/scripts/start_backend.sh"
            echo -e "\nPresione Enter para continuar..."; read -r
            ;;
        2)
            bash "$DIR/scripts/stop_backend.sh"
            echo -e "\nPresione Enter para continuar..."; read -r
            ;;
        3)
            ask_host
            bash "$DIR/scripts/check_services.sh" "$TARGET_HOST"
            echo -e "\nPresione Enter para continuar..."; read -r
            ;;
        4)
            ask_host
            bash "$DIR/scripts/run_tests.sh" "$TARGET_HOST"
            echo -e "\nPresione Enter para continuar..."; read -r
            ;;
        5)
            bash "$DIR/scripts/run_web.sh"
            echo -e "\nPresione Enter para continuar..."; read -r
            ;;
        6)
            ask_host
            bash "$DIR/scripts/run_cli.sh" "$TARGET_HOST"
            echo -e "\nPresione Enter para continuar..."; read -r
            ;;
        7)
            bash "$DIR/scripts/run_desktop.sh"
            echo -e "\nPresione Enter para continuar..."; read -r
            ;;
        0)
            echo -e "${GREEN}Hasta pronto.${RESET}"
            exit 0
            ;;
        *)
            echo -e "${YELLOW}Opción no válida. Intente de nuevo.${RESET}"
            sleep 1
            ;;
    esac
done
