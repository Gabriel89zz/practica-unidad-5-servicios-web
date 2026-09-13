#!/usr/bin/env bash
# ==============================================================================
# Script: start_backend.sh
# Descripción: Inicia los 6 microservicios en segundo plano utilizando Docker Compose.
# ==============================================================================

GREEN="\033[0;32m"
CYAN="\033[0;36m"
YELLOW="\033[1;33m"
BOLD="\033[1m"
RESET="\033[0m"

# Ir al directorio raíz del proyecto
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$DIR" || exit 1

echo -e "${BOLD}${CYAN}======================================================================${RESET}"
echo -e "${BOLD}${CYAN}   INICIANDO ECOSISTEMA DE MICROSERVICIOS (DOCKER COMPOSE)${RESET}"
echo -e "${BOLD}${CYAN}======================================================================${RESET}\n"

# Detectar comando de docker compose
if command -v docker &> /dev/null && docker compose version &> /dev/null; then
    DOCKER_CMD="docker compose"
elif command -v docker-compose &> /dev/null; then
    DOCKER_CMD="docker-compose"
else
    echo -e "${RED}[ERROR] No se encontró 'docker compose' ni 'docker-compose' instalado en el sistema.${RESET}"
    exit 1
fi

echo -e "${YELLOW}[*] Ejecutando: ${DOCKER_CMD} up -d --build...${RESET}\n"
$DOCKER_CMD up -d --build

if [ $? -eq 0 ]; then
    echo -e "\n${BOLD}${GREEN}[OK] Los microservicios han sido levantados correctamente.${RESET}\n"
    echo -e "${BOLD}Estado de los contenedores:${RESET}"
    $DOCKER_CMD ps
    echo -e "\n${CYAN}Para verificar la salud de los endpoints, ejecute: ./scripts/check_services.sh${RESET}"
else
    echo -e "\n${RED}[ERROR] Ocurrió un problema al levantar los contenedores.${RESET}"
    exit 1
fi
