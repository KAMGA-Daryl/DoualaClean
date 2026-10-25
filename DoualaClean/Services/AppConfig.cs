namespace DoualaClean.Services;

public static class AppConfig
{
    /// <summary>
    /// Cette solution (application + serveur) est livrée avec UseServer = true.
    /// false = application AUTONOME (frontend seul) : comptes, signalements et points sont stockés sur le téléphone.
    /// true  = application connectée au serveur DoualaClean.Api (voir LISEZ-MOI du serveur).
    /// </summary>
    public static readonly bool UseServer = true;
}
