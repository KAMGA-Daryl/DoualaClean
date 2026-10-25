using Android.App;
using Android.Content;
using Android.Content.PM;

namespace DoualaClean;

/// <summary>Reçoit le retour de Google (com.googleusercontent.apps.XXX:/oauth2redirect) après la connexion dans le navigateur.</summary>
[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = DoualaClean.Services.GoogleConfig.RedirectScheme,
    DataPath = "/oauth2redirect")]
public class WebAuthenticationCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
}
