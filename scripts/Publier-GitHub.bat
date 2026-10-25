@echo off
setlocal
cd /d "%~dp0.."
echo.
echo === Publication de DoualaClean sur GitHub ===
echo.
where git >nul 2>&1
if errorlevel 1 (
  echo Git n est pas installe. Telechargez-le sur https://git-scm.com puis relancez ce script.
  pause
  exit /b 1
)
for /f "delims=" %%i in ('git config --global user.name') do set GNAME=%%i
for /f "delims=" %%i in ('git config --global user.email') do set GMAIL=%%i
if "%GNAME%"=="" (
  echo Configurez d abord votre identite Git :
  echo   git config --global user.name "Prenom Nom"
  echo   git config --global user.email "vous@exemple.com"
  pause
  exit /b 1
)
echo Identite Git : %GNAME% ^<%GMAIL%^>
echo.
set /p REPO=Collez l adresse du depot GitHub (https://github.com/VOTRE-COMPTE/DoualaClean.git) : 
if "%REPO%"=="" (
  echo Adresse manquante.
  pause
  exit /b 1
)
if not exist ".git" git init
git branch -M main
git add .
echo.
echo Fichiers qui ne devraient PAS etre publies (la liste doit etre vide) :
git ls-files | findstr /i "bin/ obj/ .db uploads/"
echo --- fin de la liste ---
echo.
git commit -m "DoualaClean : projet complet (application mobile, serveur API et tableau de bord)"
git remote remove origin >nul 2>&1
git remote add origin %REPO%
git push -u origin main
echo.
echo Termine. Ouvrez la page du depot sur GitHub pour verifier.
pause
