@echo off
chcp 65001 >nul
title Practica Unidad 5 - Servicios Web Distribuidos
cls

:menu
cls
echo ======================================================================
echo    SISTEMA DISTRIBUIDO DE COMERCIO ELECTRONICO Y LOGISTICA
echo    Practica Unidad 5: Programacion en Ambiente Cliente-Servidor
echo ======================================================================
echo    [1] Iniciar Servicios Backend (docker compose up -d)
echo    [2] Detener Servicios Backend (docker compose down)
echo    [3] Ejecutar Suite de Pruebas Automatizadas (Python)
echo    --- APLICACIONES CLIENTE ---
echo    [4] Lanzar Cliente Web SPA (Tienda en Linea - HTML/CSS/JS)
echo    [5] Lanzar Cliente de Terminal / CLI (Python 3)
echo    [6] Lanzar Cliente de Escritorio (C# WinForms .NET 9)
echo    ------------------------------------------------------------------
echo    [0] Salir
echo ======================================================================
set /p opt="Seleccione una opcion [0-6]: "

if "%opt%"=="1" goto start_docker
if "%opt%"=="2" goto stop_docker
if "%opt%"=="3" goto run_tests
if "%opt%"=="4" goto run_web
if "%opt%"=="5" goto run_cli
if "%opt%"=="6" goto run_desktop
if "%opt%"=="0" goto salir
goto menu

:start_docker
echo.
echo [*] Iniciando contenedores Docker...
docker compose up -d --build
pause
goto menu

:stop_docker
echo.
echo [*] Deteniendo contenedores Docker...
docker compose down
pause
goto menu

:run_tests
echo.
set /p host_url="Ingrese la URL del servidor [Enter para http://localhost]: "
if "%host_url%"=="" set host_url=http://localhost
python docs\test_suite.py --host %host_url%
pause
goto menu

:run_web
echo.
echo [*] Abriendo Cliente Web en el navegador y sirviendo archivos...
start http://localhost:3000
python -m http.server 3000 --directory clients\web-app
pause
goto menu

:run_cli
echo.
set /p host_url="Ingrese la URL del servidor [Enter para http://localhost]: "
if "%host_url%"=="" set host_url=http://localhost
python clients\cli-tool\cli.py --host %host_url%
pause
goto menu

:run_desktop
echo.
echo [*] Compilando y ejecutando Cliente de Escritorio WinForms...
dotnet run --project clients\desktop-app\DesktopApp.csproj
pause
goto menu

:salir
exit /b 0
