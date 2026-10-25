@echo off
echo Adresses IPv4 de ce PC (utiliser celle du reseau Wi-Fi) :
echo.
ipconfig | findstr /R /C:"IPv4"
echo.
echo Dans l application, touchez "Serveur" et saisissez : http://ADRESSE:5080
echo Exemple : http://192.168.1.20:5080
pause
