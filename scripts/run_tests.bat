@echo off
chcp 65001 >nul
cd /d "%~dp0\.."
if "%~1"=="" (
    set /p host_url="Ingrese la URL del servidor [Enter para http://localhost]: "
) else (
    set host_url=%~1
)
if "%host_url%"=="" set host_url=http://localhost
python docs\test_suite.py --host %host_url%
pause
