using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DoualaClean.Services;

/// <summary>Connexion Google : ouvre le navigateur (OAuth + PKCE) et renvoie le jeton d'identité (id_token).</summary>
public static class GoogleSignIn
{
    public static async Task<(string? IdToken, string Error)> GetIdTokenAsync()
    {
        if (!GoogleConfig.IsConfigured)
            return (null, "Connexion Google non configurée : renseignez votre ID client dans Services/GoogleConfig.cs (voir LISEZ-MOI).");

        try
        {
            var verifier = Base64Url(RandomNumberGenerator.GetBytes(48));
            var challenge = Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
            var state = Base64Url(RandomNumberGenerator.GetBytes(16));

            var authUrl = "https://accounts.google.com/o/oauth2/v2/auth"
                + $"?client_id={Uri.EscapeDataString(GoogleConfig.ClientId)}"
                + $"&redirect_uri={Uri.EscapeDataString(GoogleConfig.RedirectUri)}"
                + "&response_type=code"
                + $"&scope={Uri.EscapeDataString("openid email profile")}"
                + $"&code_challenge={challenge}&code_challenge_method=S256"
                + $"&state={state}";

            var result = await WebAuthenticator.Default.AuthenticateAsync(new WebAuthenticatorOptions
            {
                Url = new Uri(authUrl),
                CallbackUrl = new Uri(GoogleConfig.RedirectUri)
            });

            if (!result.Properties.TryGetValue("state", out var returnedState) || returnedState != state)
                return (null, "Réponse Google invalide (état différent). Réessayez.");
            if (!result.Properties.TryGetValue("code", out var code) || string.IsNullOrEmpty(code))
                return (null, "Connexion Google annulée.");

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var resp = await http.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = GoogleConfig.ClientId,
                ["code"] = code,
                ["code_verifier"] = verifier,
                ["redirect_uri"] = GoogleConfig.RedirectUri,
                ["grant_type"] = "authorization_code"
            }));
            var json = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return (null, "Google a refusé la connexion. Vérifiez la configuration de l'ID client.");

            using var doc = JsonDocument.Parse(json);
            var idToken = doc.RootElement.GetProperty("id_token").GetString();
            return string.IsNullOrEmpty(idToken) ? (null, "Jeton Google manquant.") : (idToken, "");
        }
        catch (TaskCanceledException) { return (null, "Connexion Google annulée."); }
        catch (Exception ex) { return (null, "Connexion Google impossible : " + ex.Message); }
    }

    /// <summary>Lit l'email et le nom contenus dans le jeton (sans vérification : réservé au mode autonome).</summary>
    public static (string Email, string Name) ReadProfile(string idToken)
    {
        var part = idToken.Split('.')[1].Replace('-', '+').Replace('_', '/');
        part = part.PadRight(part.Length + (4 - part.Length % 4) % 4, '=');
        using var doc = JsonDocument.Parse(Convert.FromBase64String(part));
        var root = doc.RootElement;
        var email = root.TryGetProperty("email", out var e) ? e.GetString() ?? "" : "";
        var name = root.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
        return (email, name);
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
