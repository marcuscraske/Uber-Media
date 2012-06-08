echo off
cls
title Clean Bin Folders
echo Base-path: '%~dp0'
echo.
echo.
echo Cleaning Code__Server...
echo.
echo.
rd "%~dp0Code__Server\bin" /s /q
echo.
echo.
echo Cleaning WebsiteLauncher...
echo.
echo.
rd "%~dp0WebsiteLauncher\bin" /s /q
echo.
echo.
echo Cleaning UberMediaSharedLibrary...
echo.
echo.
rd "%~dp0UberMediaSharedLibrary\bin" /s /q
echo.
echo.
pause