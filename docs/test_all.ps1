# Script PowerShell para Ejecución Automatizada de Pruebas
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  ECOSISTEMA DE SERVICIOS WEB DISTRIBUIDOS (REST & SOAP)   " -ForegroundColor Yellow
Write-Host "       Práctica Unidad 5 - Cliente-Servidor 7mo Sem        " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$services = @(
    @{ Name = "1. Java (Inventario)"; Port = 8081; RestPath = "/api/v1/productos"; WsdlPath = "/ws/inventario.wsdl" },
    @{ Name = "2. Python (Envios)"; Port = 8082; RestPath = "/api/v1/envios"; WsdlPath = "/soap/envios?wsdl" },
    @{ Name = "3. PHP (Ordenes)"; Port = 8083; RestPath = "/api/v1/ordenes"; WsdlPath = "/soap/ordenes.wsdl" },
    @{ Name = "4. Ruby (Notificaciones)"; Port = 8084; RestPath = "/api/v1/destinatarios"; WsdlPath = "/soap/notificaciones/wsdl" },
    @{ Name = "5. C# (Facturacion)"; Port = 8085; RestPath = "/api/v1/facturas"; WsdlPath = "/soap/FacturacionService.svc?wsdl" },
    @{ Name = "6. VB.NET (Auditoria)"; Port = 8086; RestPath = "/api/v1/logs"; WsdlPath = "/soap/AuditoriaService.svc?wsdl" }
)

$token = "Bearer test_token_2026"

foreach ($s in $services) {
    Write-Host "`n>> Probando $($s.Name) en puerto $($s.Port)..." -ForegroundColor Green
    $baseUrl = "http://localhost:$($s.Port)"

    # 1. Probar REST sin token (Debe fallar con 401)
    try {
        $resp = Invoke-WebRequest -Uri "$baseUrl$($s.RestPath)" -Method GET -UseBasicParsing -ErrorAction Stop
        Write-Host "   [FAIL] Se esperaba 401 Unauthorized sin token, pero devolvió $($resp.StatusCode)" -ForegroundColor Red
    } catch {
        if ($_.Exception.Response.StatusCode -eq 401) {
            Write-Host "   [PASS] 401 Unauthorized verificado sin credenciales." -ForegroundColor Gray
        } else {
            Write-Host "   [AVISO] No se pudo contactar al servicio: $($_.Exception.Message)" -ForegroundColor Yellow
            continue
        }
    }

    # 2. Probar REST con token
    try {
        $headers = @{ "Authorization" = $token }
        $resp = Invoke-RestMethod -Uri "$baseUrl$($s.RestPath)" -Headers $headers -Method GET -UseBasicParsing
        Write-Host "   [PASS] REST API responde HTTP 200 con Bearer Token autorizada." -ForegroundColor Cyan
    } catch {
        Write-Host "   [FAIL] Error en petición autorizada: $($_.Exception.Message)" -ForegroundColor Red
    }

    # 3. Probar WSDL
    try {
        $wsdl = Invoke-WebRequest -Uri "$baseUrl$($s.WsdlPath)" -Method GET -UseBasicParsing
        if ($wsdl.Content -match "<.*definitions") {
            Write-Host "   [PASS] Contrato WSDL recuperado con éxito." -ForegroundColor Magenta
        } else {
            Write-Host "   [FAIL] Contenido devuelto no parece un WSDL válido." -ForegroundColor Red
        }
    } catch {
        Write-Host "   [FAIL] Error al obtener WSDL: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host "              FIN DEL REPORTE DE PRUEBAS                  " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
