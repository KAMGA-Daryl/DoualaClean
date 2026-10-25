# DoualaClean – application + serveur local

## Contenu
- `DoualaClean/`      : application mobile .NET MAUI (Android)
- `DoualaClean.Api/`  : serveur ASP.NET Core (API + base SQLite + tableau de bord admin)
- `DoualaClean.sln`   : solution contenant les deux projets

## Tester en local : voir les comptes et les photos sur le PC
Cette solution est livrée en **mode serveur** (`DoualaClean/Services/AppConfig.cs` : `UseServer = true`).
Le guide détaillé est dans **DoualaClean-Guide-Backend.pdf**. Résumé :
1. **Serveur** : projet de démarrage `DoualaClean.Api` > F5 (ou double-clic sur `scripts\Lancer-Serveur.bat`).
   Tableau de bord : http://localhost:5080/admin/ — admin : `admin@doualaclean.local` / `Admin2026!`
2. **Application** : projet `DoualaClean` > émulateur Android ou téléphone > ▶.
   - Émulateur : l'adresse par défaut `http://10.0.2.2:5080` suffit.
   - Téléphone (même Wi-Fi) : `scripts\Afficher-IP.bat`, puis dans l'app « ⚙ Serveur » > `http://IP-DU-PC:5080`.
     Si ça ne répond pas : `scripts\Ouvrir-Pare-feu.bat` (en administrateur).
   - Bouton **Tester la connexion au serveur** sur l'écran de connexion pour vérifier.
3. Créer un compte dans l'app, faire un signalement avec photo + GPS : le tableau de bord (actualisation automatique toutes les 10 s)
   affiche le nouvel utilisateur, le signalement, la photo et la position (badge « Nouveau »).
Pour lancer les deux en même temps : clic droit sur la solution > Propriétés > Projets de démarrage multiples.

## Où sont les données ?
- Base : `DoualaClean.Api/doualaclean.db` (SQLite). Supprimer ce fichier = repartir de zéro.
- Photos : dossier `DoualaClean.Api/uploads/`.

## Avant une vraie mise en ligne
HTTPS obligatoire, nouvelle clé `Jwt:Key`, nouveau mot de passe admin, envoi d'emails (mot de passe oublié),
protection des photos, limitation de débit, politique de confidentialité (photos + positions = données personnelles).

## Connexion Google (bouton « Continuer avec Google » de l'écran Récupération)
Le bouton est en place, mais Google exige un identifiant que seul ton compte peut créer (non fourni, donc non testé ici) :
1. https://console.cloud.google.com > nouveau projet > **Écran de consentement OAuth** (type Externe ; en mode « Test », ajoute ton adresse Gmail dans « Utilisateurs de test »).
2. **Identifiants > Créer des identifiants > ID client OAuth > Android** :
   - Nom du package : `com.doualaclean.app`
   - Empreinte SHA-1 de la clé de débogage de Visual Studio :
     `keytool -list -v -keystore "%LOCALAPPDATA%\Xamarin\Mono for Android\debug.keystore" -alias androiddebugkey -storepass android -keypass android`
     (keytool se trouve dans le dossier bin du JDK installé avec Visual Studio)
   - Si Google propose « Activer le schéma d'URI personnalisé » (paramètres avancés), l'activer.
3. Copier l'ID client (ex. `1234-abcd.apps.googleusercontent.com`) :
   - `DoualaClean/Services/GoogleConfig.cs` : `ClientIdPrefix = "1234-abcd"` (la partie avant `.apps.googleusercontent.com`)
   - `DoualaClean.Api/appsettings.json` : `"Google": { "ClientIds": [ "1234-abcd.apps.googleusercontent.com" ] }`
4. Régénérer la solution et relancer. L'émulateur doit avoir Chrome / Google Play.
Tant que ce n'est pas configuré, le bouton affiche un message clair au lieu de planter.
Fonctionnement : le téléphone obtient un jeton Google (navigateur + PKCE), le serveur le vérifie auprès de Google, puis ouvre le compte (créé automatiquement s'il n'existe pas). Les comptes administrateur sont exclus de ce mode.

## Mode autonome (frontend seul)
Pour une application sans serveur (comptes et signalements stockés sur le téléphone), passer `UseServer` à `false`
(compte démo : daryl.kamga@example.com / Douala2026). Le projet frontend seul est fourni séparément (DoualaClean-Frontend.zip).

## Publier sur GitHub en une fois
`scripts\Publier-GitHub.bat` : crée le dépôt local, fait un commit unique avec tout le projet et l'envoie sur le dépôt GitHub indiqué
(créer d'abord un dépôt PUBLIC vide sur github.com, et configurer `git config --global user.name` / `user.email`).
