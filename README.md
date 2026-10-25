# DoualaClean

Application mobile .NET MAUI (Android) de signalement des dépôts sauvages d'ordures à Douala, avec son serveur ASP.NET Core
et un tableau de bord administrateur.

- **Application mobile** (`DoualaClean/`) : 21 écrans XAML fidèles aux maquettes Figma, logique MVVM en C#.
  Connexion / inscription, signalement avec **photo** et **GPS**, carte des bacs, guide de tri, suivi, Green Points,
  mode sombre, bouton « Continuer avec Google » (facultatif).
- **Serveur** (`DoualaClean.Api/`) : API REST, base SQLite, authentification JWT, stockage des photos.
- **Tableau de bord admin** (`DoualaClean.Api/wwwroot/admin`) : utilisateurs, signalements avec photos, carte, changement de statut.

## Lancer
1. Visual Studio avec les charges de travail **.NET MAUI** et **ASP.NET et développement web** (SDK .NET 9). Ouvrir `DoualaClean.sln`.
2. **Serveur** : projet `DoualaClean.Api` > F5 (ou `scripts\Lancer-Serveur.bat`). Tableau de bord : http://localhost:5080/admin/
   (compte administrateur de démonstration : `admin@doualaclean.local` / `Admin2026!` — à changer avant toute mise en ligne).
3. **Application** : projet `DoualaClean` > téléphone Android (débogage USB) ou émulateur > ▶.
   - Émulateur : adresse du serveur `http://10.0.2.2:5080` (par défaut).
   - Téléphone : « ⚙ Serveur » sur l'écran de connexion > `http://IP-DU-PC:5080` (voir `scripts\Afficher-IP.bat`, `scripts\Ouvrir-Pare-feu.bat`).
4. Cartes : clé Google Maps à renseigner dans `DoualaClean/Platforms/Android/AndroidManifest.xml` (`VOTRE_CLE_GOOGLE_MAPS`).

## Modes de l'application
`DoualaClean/Services/AppConfig.cs` : `UseServer = true` (avec serveur) ou `false` (autonome : données sur le téléphone,
compte de démonstration `daryl.kamga@example.com` / `Douala2026`).

## Structure
```
DoualaClean/            Application MAUI : Views (XAML), ViewModels, Models, Services, Resources, Platforms
DoualaClean.Api/        Serveur ASP.NET Core : Program.cs, Data.cs, Dtos.cs, Security.cs, wwwroot/admin
scripts/                Lancer-Serveur.bat, Afficher-IP.bat, Ouvrir-Pare-feu.bat
```

## Limites connues
Langue FR/EN enregistrée mais textes non traduits ; notifications push : préférences seulement ; email de récupération non envoyé ;
bacs de collecte : données d'exemple ; serveur en HTTP pour les tests locaux (HTTPS à prévoir pour une mise en ligne).

## Sécurité
Les valeurs de `appsettings.json` (clé JWT, mot de passe administrateur) sont des **exemples** pour les tests locaux.
Ne jamais publier de vrais secrets dans un dépôt public : en production, les fournir par variables d'environnement.
