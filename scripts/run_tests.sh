#!/usr/bin/env bash
# ==============================================================================
# Script: run_tests.sh
# Descripción: Ejecuta la suite de pruebas automatizadas End-to-End en Python
#              validando los 6 microservicios en REST y SOAP.
# Uso:
#   ./scripts/run_tests.sh                       # Prueba contra localhost
#   ./scripts/run_tests.sh http://IP_DEBIAN      # Prueba contra servidor remoto
#   ./scripts/run_tests.sh --host http://IP_DEBIAN
# ==============================================================================

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TEST_SCRIPT="${DIR}/docs/test_suite.py"

CYAN="\033[0;36m"
BOLD="\033[1m"
RESET="\033[0m"

echo -e "${BOLD}${CYAN}======================================================================${RESET}"
echo -e "${BOLD}${CYAN}   SUITE DE PRUEBAS AUTOMATIZADAS END-TO-END (REST & SOAP)${RESET}"
echo -e "${BOLD}${CYAN}======================================================================${RESET}\n"

if command -v python3 &> /dev/null; then
    PY="python3"
elif command -v python &> /dev/null; then
    PY="python"
else
    echo "Error: Python 3 no encontrado en el PATH."
    exit 1
fi

if [ "$#" -eq 1 ] && [[ "$1" == http* ]]; then
    exec $PY "$TEST_SCRIPT" --host "$1"
fi

exec $PY "$TEST_SCRIPT" "$@"
