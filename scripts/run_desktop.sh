#!/usr/bin/env bash
# ==============================================================================
# Script: run_desktop.sh
# Descripción: Ejecuta o compila la Aplicación de Escritorio WinForms (.NET 9 C#).
# Nota: WinForms es una interfaz gráfica de escritorio. Si se ejecuta en Windows
#       o WSL con soporte gráfico (WSLg), se abrirá la ventana del cliente.
#       En entornos de servidor Linux sin pantalla (headless), compilará el código.
# ==============================================================================

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CSPROJ="${DIR}/clients/desktop-app/DesktopApp.csproj"

CYAN="\033[0;36m"
GREEN="\033[0;32m"
YELLOW="\033[1;33m"
RED="\033[0;31m"
BOLD="\033[1m"
RESET="\033[0m"

echo -e "${BOLD}${CYAN}======================================================================${RESET}"
echo -e "${BOLD}${CYAN}   CLIENTE DE ESCRITORIO WINFORMS (.NET 9 C#)${RESET}"
echo -e "${BOLD}${CYAN}======================================================================${RESET}\n"

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}[ERROR] El SDK de .NET no se encuentra instalado en el PATH.${RESET}"
    echo -e "Instale el SDK de .NET 9 desde: https://dotnet.microsoft.com/download"
    exit 1
fi

DOTNET_VER=$(dotnet --version 2>/dev/null)
echo -e "${GREEN}[OK] .NET SDK detectado: v${DOTNET_VER}${RESET}"

# Detectar si estamos en un entorno sin pantalla gráfica
OS="$(uname -s 2>/dev/null || echo "Windows")"
if [[ "$OS" == "Linux"* ]] && [ -z "$DISPLAY" ] && [ -z "$WAYLAND_DISPLAY" ]; then
    echo -e "\n${YELLOW}[AVISO DE ENTORNO SERVIDOR / HEADLESS]${RESET}"
    echo -e "Se detectó un sistema Linux sin servidor gráfico (sin \$DISPLAY ni Wayland)."
    echo -e "Las ventanas Windows Forms (WinForms) requieren un entorno de escritorio (Windows o WSLg)."
    echo -e "\n${YELLOW}[*] Validando compilación del cliente de escritorio con 'dotnet build'...${RESET}\n"
    dotnet build "$CSPROJ"
    if [ $? -eq 0 ]; then
        echo -e "\n${GREEN}[OK] El cliente de escritorio compila al 100% sin errores.${RESET}"
        echo -e "Para ejecutar la interfaz gráfica visual, ejecútelo desde una máquina con Windows:"
        echo -e "  cd clients/desktop-app && dotnet run"
    fi
    exit 0
fi

echo -e "${YELLOW}[*] Ejecutando: dotnet run --project ${CSPROJ}...${RESET}\n"
dotnet run --project "$CSPROJ"
