#!/usr/bin/env bash
# ==============================================================================
# Script: check_services.sh
# Descripción: Verifica la disponibilidad y estado de los 6 microservicios
#              (Java, Python, PHP, Ruby, C#, VB.NET) en REST y SOAP/WSDL.
# Uso: ./scripts/check_services.sh [http://IP_O_HOST]
# ==============================================================================

HOST="${1:-http://localhost}"
HOST="${HOST%/}"

# Colores ANSI
GREEN="\033[0;32m"
RED="\033[0;31m"
YELLOW="\033[1;33m"
CYAN="\033[0;36m"
BOLD="\033[1m"
RESET="\033[0m"

echo -e "${BOLD}${CYAN}======================================================================${RESET}"
echo -e "${BOLD}${CYAN}   VERIFICACION DE SALUD DE MICROSERVICIOS (${HOST})${RESET}"
echo -e "${BOLD}${CYAN}======================================================================${RESET}\n"

# Array de servicios: Nombre | Puerto | Path REST | Path WSDL
SERVICES=(
    "Java Spring Boot|8081|/api/v1/productos|/ws/inventario.wsdl"
    "Python FastAPI|8082|/api/v1/envios|/soap/envios?wsdl"
    "PHP Slim 4|8083|/api/v1/ordenes|/soap/ordenes.wsdl"
    "Ruby Sinatra|8084|/api/v1/destinatarios|/soap/notificaciones/wsdl"
    "C# .NET 9|8085|/api/v1/facturas|/soap/FacturacionService.svc?wsdl"
    "VB.NET .NET 9|8086|/api/v1/logs|/soap/AuditoriaService.svc?wsdl"
)

TOTAL=6
ONLINE=0

for item in "${SERVICES[@]}"; do
    IFS='|' read -r NAME PORT REST_PATH WSDL_PATH <<< "$item"
    URL_BASE="${HOST}:${PORT}"
    
    echo -e "${BOLD}▶ [Puerto ${PORT}] ${NAME}${RESET}"
    
    # 1. Probar conectividad base / REST
    REST_URL="${URL_BASE}${REST_PATH}"
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 3 -m 5 -H "Authorization: Bearer test_token_2026" "${REST_URL}" 2>/dev/null)
    
    if [ "$HTTP_CODE" = "200" ]; then
        echo -e "   REST API:   ${GREEN}[OK] HTTP 200${RESET} -> ${REST_URL}"
    elif [ "$HTTP_CODE" = "401" ]; then
        echo -e "   REST API:   ${YELLOW}[PROTEGIDO] HTTP 401${RESET} (Filtro de seguridad activo)"
    elif [ "$HTTP_CODE" = "000" ] || [ -z "$HTTP_CODE" ]; then
        echo -e "   REST API:   ${RED}[INACCESIBLE] Conexión rechazada o timeout${RESET} (${REST_URL})"
    else
        echo -e "   REST API:   ${YELLOW}[RESPONDE] HTTP ${HTTP_CODE}${RESET} -> ${REST_URL}"
    fi

    # 2. Probar contrato SOAP WSDL
    WSDL_URL="${URL_BASE}${WSDL_PATH}"
    WSDL_CODE=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 3 -m 5 "${WSDL_URL}" 2>/dev/null)
    
    if [ "$WSDL_CODE" = "200" ]; then
        echo -e "   SOAP WSDL:  ${GREEN}[OK] HTTP 200${RESET} -> ${WSDL_URL}"
    else
        echo -e "   SOAP WSDL:  ${RED}[FALLO] HTTP ${WSDL_CODE}${RESET} -> ${WSDL_URL}"
    fi

    # Contabilizar online si responde REST o WSDL
    if [ "$HTTP_CODE" != "000" ] && [ -n "$HTTP_CODE" ]; then
        ((ONLINE++))
    fi
    echo ""
done

echo -e "${BOLD}${CYAN}----------------------------------------------------------------------${RESET}"
if [ "$ONLINE" -eq "$TOTAL" ]; then
    echo -e "${BOLD}${GREEN}[RESUMEN] Los 6 microservicios están en línea y respondiendo (6/6).${RESET}"
else
    echo -e "${BOLD}${YELLOW}[RESUMEN] ${ONLINE} de ${TOTAL} servicios disponibles.${RESET}"
    echo -e "Asegúrese de haber ejecutado 'docker compose up -d' o verificar las reglas de firewall (UFW)."
fi
echo -e "${BOLD}${CYAN}======================================================================${RESET}"
