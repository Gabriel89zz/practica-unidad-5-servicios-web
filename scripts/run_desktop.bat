@echo off
chcp 65001 >nul
cd /d "%~dp0\.."
echo [*] Compilando y ejecutando Cliente de Escritorio WinForms (.NET 9 C#)...
dotnet run --project clients\desktop-app\DesktopApp.csproj
pause
