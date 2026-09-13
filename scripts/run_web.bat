@echo off
chcp 65001 >nul
cd /d "%~dp0\.."
echo [*] Iniciando servidor web local para Cliente Web SPA...
start http://localhost:3000
python -m http.server 3000 --directory clients\web-app
pause
