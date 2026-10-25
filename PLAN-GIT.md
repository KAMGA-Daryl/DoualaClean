# Plan de commits pour le dépôt GitHub (à adapter à l'énoncé)

Les phases A, B et C sont une PROPOSITION : remplace les titres et regroupe les fichiers selon l'énoncé de l'examen.
Les commits portent TON nom et la VRAIE date. Ne modifie jamais les dates ni l'historique après le push.
Les premiers commits peuvent ne pas compiler seuls : c'est normal.
Dépôt PUBLIC : ne mets aucun vrai mot de passe ni vraie clé dans les fichiers (appsettings.json contient des exemples).

## Une seule fois
```
git --version
git config --global user.name "Prénom Nom"
git config --global user.email "ton.email@exemple.com"
```
github.com > New repository > `DoualaClean` > **Public** > ne rien cocher > Create.

## Dans le dossier de la solution
```
git init
git branch -M main

git add .gitignore README.md
git add DoualaClean/DoualaClean.csproj DoualaClean/GlobalUsings.cs DoualaClean/MauiProgram.cs
git add DoualaClean/App.xaml DoualaClean/App.xaml.cs DoualaClean/AppShell.xaml DoualaClean/AppShell.xaml.cs
git add DoualaClean/Resources DoualaClean/Controls DoualaClean/Platforms
git commit -m "Phase A : structure du projet, charte graphique et navigation"

git add "DoualaClean/Views/*.xaml"
git commit -m "Phase B : integration des interfaces XAML (21 ecrans)"

git add DoualaClean/Models DoualaClean/ViewModels "DoualaClean/Views/*.xaml.cs"
git add DoualaClean/Services/IServices.cs DoualaClean/Services/AppConfig.cs
git add DoualaClean/Services/LocalAuthService.cs DoualaClean/Services/LocalReportService.cs
git add DoualaClean/Services/StaticServices.cs DoualaClean/Services/SettingsService.cs
git commit -m "Phase C : logique MVVM, modeles et services"

git add DoualaClean/Services/LocationHelper.cs
git commit -m "Fonctionnalite : position GPS reelle, adresse et photo"

git add DoualaClean/Services/GoogleConfig.cs DoualaClean/Services/GoogleSignIn.cs
git commit -m "Fonctionnalite : bouton Continuer avec Google"

git add DoualaClean.Api DoualaClean.sln
git commit -m "Fonctionnalite : serveur API, base de donnees, photos et tableau de bord admin"

git add DoualaClean/Services/ApiClient.cs DoualaClean/Services/ApiAuthService.cs DoualaClean/Services/ApiReportService.cs
git commit -m "Fonctionnalite : application connectee au serveur"

git add .
git commit -m "Scripts Windows et documentation"
```

## Vérifier et publier
```
git status
git log --oneline
git remote add origin https://github.com/VOTRE-COMPTE/DoualaClean.git
git push -u origin main
```
`git status` doit indiquer « nothing to commit » avant le push. GitHub n'accepte plus le mot de passe du compte :
une fenêtre du navigateur s'ouvre pour t'authentifier (ou utilise un Personal Access Token).
Sur la page du dépôt, clique sur « N commits » : un commit distinct par phase doit apparaître, dans l'ordre.
Vérifie que le dépôt est public (pas de cadenas), puis copie son adresse pour l'espace du cours.
