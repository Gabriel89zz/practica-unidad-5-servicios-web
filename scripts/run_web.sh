#!/usr/bin/env bash
# ==============================================================================
# Script: run_web.sh
# Descripción: Inicia un servidor web local para servir el Cliente Web SPA
#              y opcionalmente abre el navegador predeterminado.
# Uso: ./scripts/run_web.sh [PUERTO]
# ==============================================================================

PORT="${1:-3000}"
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
WEB_DIR="${DIR}/clients/web-app"

GREEN="\033[0;32m"
CYAN="\033[0;36m"
YELLOW="\033[1;33m"
BOLD="\033[1m"
RESET="\033[0m"

echo -e "${BOLD}${CYAN}======================================================================${RESET}"
echo -e "${BOLD}${CYAN}   LANZANDO CLIENTE WEB SPA (TIENDA DISTRIBUIDA EN LINEA)${RESET}"
echo -e "${BOLD}${CYAN}======================================================================${RESET}\n"

if [ ! -d "$WEB_DIR" ]; then
    echo -e "${RED}[ERROR] No se encontró el directorio del cliente web: ${WEB_DIR}${RESET}"
    exit 1
fi

# Detectar Python
if command -v python3 &> /dev/null; then
    PY="python3"
elif command -v python &> /dev/null; then
    PY="python"
else
    echo -e "${RED}[ERROR] Se requiere Python 3 para iniciar el servidor web local.${RESET}"
    echo -e "Como alternativa, puede abrir directamente el archivo:"
    echo -e "  ${WEB_DIR}/index.html"
    exit 1
fi

URL="http://localhost:${PORT}"
echo -e "${GREEN}[OK] Directorio estático: ${WEB_DIR}${RESET}"
echo -e "${BOLD}${YELLOW}▶ Servidor web iniciado en: ${URL}${RESET}"
echo -e "Abra la URL anterior en su navegador (o use la IP de su red local)."
echo -e "${CYAN}Tip: En la Web App puede cambiar la IP del servidor de 'localhost' a la IP de su Debian Server.${RESET}"
echo -e "Presione ${BOLD}Ctrl+C${RESET} para detener el servidor web.\n"

# Intentar abrir el navegador automáticamente si existe entorno gráfico
if command -v xdg-open &> /dev/null; then
    xdg-open "$URL" 2>/dev/null &
elif command -v open &> /dev/null; then
    open "$URL" 2>/dev/null &
elif command -v explorer.exe &> /dev/null; then
    explorer.exe "$URL" 2>/dev/null &
fi

$PY -m http.server "$PORT" --directory "$WEB_DIR"
