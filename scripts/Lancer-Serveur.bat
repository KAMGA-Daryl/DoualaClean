@echo off
cd /d "%~dp0..\DoualaClean.Api"
echo Demarrage du serveur DoualaClean...
echo Tableau de bord : http://localhost:5080/admin/
echo (laisser cette fenetre ouverte ; Ctrl+C pour arreter)
echo.
dotnet run
pause
