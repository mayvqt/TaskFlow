@echo off
title Quick Test App
color 0A
echo Starting Quick Test Application...
echo Time: %time%
echo.
ping localhost -n 3 >nul
echo Application initialized successfully!
echo.
echo This app will run for 60 seconds then exit automatically.
echo You can also stop it manually using TaskFlow.
echo.

timeout /t 60 /nobreak
echo.
echo Application completed normally. Exiting...
exit /b 0