@echo off
chcp 65001 >nul
title PolyKafe Web Runner (LAB03 - Port 5088)
cd /d "%~dp0"
echo ==============================================
echo   DANG KHOI DONG BAN LAB03 (PORT 5088)...
echo ==============================================
echo.
start "" "http://localhost:5088/Kiosk"
dotnet run --project src/CafePOSManagement.Web --launch-profile http
pause
