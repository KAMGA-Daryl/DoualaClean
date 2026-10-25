@echo off
net session >nul 2>&1
if %errorLevel% neq 0 (
  echo Faites un clic droit sur ce fichier puis "Executer en tant qu administrateur".
  pause
  exit /b
)
netsh advfirewall firewall add rule name="DoualaClean API 5080" dir=in action=allow protocol=TCP localport=5080 profile=private
echo.
echo Port 5080 autorise pour les reseaux prives.
pause
