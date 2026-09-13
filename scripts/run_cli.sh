#!/usr/bin/env bash
# ==============================================================================
# Script: run_cli.sh
# Descripción: Ejecuta el Cliente de Consola / Terminal CLI en modo interactivo
#              o con argumentos directos.
# Uso: 
#   ./scripts/run_cli.sh                          # Menú interactivo
#   ./scripts/run_cli.sh http://IP_DEBIAN         # Menú interactivo apuntando a IP
#   ./scripts/run_cli.sh --host http://IP --help  # Argumentos directos
# ==============================================================================

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CLI_FILE="${DIR}/clients/cli-tool/cli.py"

CYAN="\033[0;36m"
BOLD="\033[1m"
RESET="\033[0m"

# Detectar comando Python
if command -v python3 &> /dev/null; then
    PY="python3"
elif command -v python &> /dev/null; then
    PY="python"
else
    echo "Error: Python 3 no encontrado en el PATH."
    exit 1
fi

# Si se pasó una sola URL como primer parámetro sin guiones (ej. ./run_cli.sh http://192.168.1.10)
if [ "$#" -eq 1 ] && [[ "$1" == http* ]]; then
    exec $PY "$CLI_FILE" --host "$1"
fi

# Si se pasaron argumentos de opciones (ej. --service, --help, etc.)
if [ "$#" -gt 0 ]; then
    exec $PY "$CLI_FILE" "$@"
fi

# Modo sin argumentos: Preguntar por host o usar localhost
echo -e "${BOLD}${CYAN}======================================================================${RESET}"
echo -e "${BOLD}${CYAN}   CLIENTE DE TERMINAL / CLI (CONSUMO RUBY Y VB.NET)${RESET}"
echo -e "${BOLD}${CYAN}======================================================================${RESET}"
read -r -p "Ingrese la URL del servidor [Enter para usar http://localhost]: " USER_HOST
USER_HOST="${USER_HOST:-http://localhost}"

exec $PY "$CLI_FILE" --host "$USER_HOST"
