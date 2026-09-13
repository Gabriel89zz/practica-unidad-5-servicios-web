#!/usr/bin/env bash
# ==============================================================================
# Script: stop_backend.sh
# Descripción: Detiene todos los contenedores de los microservicios.
# ==============================================================================

YELLOW="\033[1;33m"
GREEN="\033[0;32m"
CYAN="\033[0;36m"
BOLD="\033[1m"
RESET="\033[0m"

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$DIR" || exit 1

echo -e "${BOLD}${CYAN}======================================================================${RESET}"
echo -e "${BOLD}${CYAN}   DETENIENDO ECOSISTEMA DE MICROSERVICIOS${RESET}"
echo -e "${BOLD}${CYAN}======================================================================${RESET}\n"

if command -v docker &> /dev/null && docker compose version &> /dev/null; then
    DOCKER_CMD="docker compose"
elif command -v docker-compose &> /dev/null; then
    DOCKER_CMD="docker-compose"
else
    echo -e "${YELLOW}[AVISO] Docker no encontrado en el PATH.${RESET}"
    exit 1
fi

$DOCKER_CMD down
echo -e "\n${BOLD}${GREEN}[OK] Contenedores detenidos y recursos liberados.${RESET}"
