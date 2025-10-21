@echo off
echo TaskFlow Test Application Started at %date% %time%
echo.
echo This is a test application for TaskFlow monitoring.
echo Press Ctrl+C to stop or wait for automatic timeout...
echo.

:loop
echo Running... %time%
timeout /t 5 /nobreak >nul
goto loop