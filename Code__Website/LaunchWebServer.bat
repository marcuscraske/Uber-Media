set path=%~dp0
set path=%path:~0,-1%
echo off
cls
echo Running Portable Server
echo =========================
echo Visit http://127.0.0.1 or the IP of this machine; the website may take up to thirty-seconds to load.
echo .
echo .
Webserver\UltiDevCassinWebServer2a.exe /run "%path%" "Default.aspx" "80" nobrowser