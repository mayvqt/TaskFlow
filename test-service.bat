@echo off
title TaskFlow Test Service
echo ========================================
echo      TaskFlow Test Service
echo ========================================
echo.
echo Service started at: %date% %time%
echo Process ID: %RANDOM%
echo.
echo This application will run indefinitely.
echo Use TaskFlow to stop/restart this service.
echo.

:heartbeat
echo [%time%] Service heartbeat - Running normally...
timeout /t 10 /nobreak >nul
if errorlevel 1 goto shutdown
goto heartbeat

:shutdown
echo.
echo Service shutting down at: %date% %time%
echo Goodbye!
pause