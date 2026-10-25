namespace DoualaClean.Services;

/// <summary>
/// Configuration de la connexion Google.
/// 1) Google Cloud Console > Identifiants > Créer un ID client OAuth de type "Android"
///    (nom du package : com.doualaclean.app + empreinte SHA-1 de la clé de débogage).
/// 2) Copier ici la partie de l'ID client AVANT ".apps.googleusercontent.com".
/// 3) Mettre l'ID client complet dans DoualaClean.Api/appsettings.json (Google:ClientIds).
/// </summary>
public static class GoogleConfig
{
    public const string ClientIdPrefix = "VOTRE-CLIENT-ID";

    public const string ClientId = ClientIdPrefix + ".apps.googleusercontent.com";
    public const string RedirectScheme = "com.googleusercontent.apps." + ClientIdPrefix;
    public const string RedirectUri = RedirectScheme + ":/oauth2redirect";

    public static bool IsConfigured => ClientIdPrefix != "VOTRE-CLIENT-ID" && ClientIdPrefix.Length > 10;
}
