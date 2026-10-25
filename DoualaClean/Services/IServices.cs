namespace DoualaClean.Services;

public interface IAuthService
{
    AppUser? CurrentUser { get; }
    bool IsLoggedIn { get; }
    event Action? UserChanged;
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string email, string fullName, string password);
    Task<AuthResult> SendRecoveryAsync(string email);
    /// <summary>Connexion / récupération du compte via Google (OAuth + PKCE puis vérification par le serveur).</summary>
    Task<AuthResult> LoginWithGoogleAsync();
    /// <summary>Recharge le profil et les points depuis le serveur.</summary>
    Task RefreshAsync();
    void Logout();
    /// <summary>Demande au serveur une récompense unique (ex. "guide"). Le serveur décide des points.</summary>
    Task AwardOnceAsync(string key);
}

public interface IReportService
{
    Task<IReadOnlyList<WasteReport>> GetMineAsync();
    WasteReport? GetById(int id);
    Task<WasteReport> SubmitAsync(ReportDraft draft);
}

public interface IBinService
{
    IReadOnlyList<CollectionBin> GetAll();
    CollectionBin? GetById(int id);
}

public interface ISortingGuideService
{
    IReadOnlyList<SortingCategory> GetAll();
    SortingCategory? GetByKey(string key);
}

public interface ISettingsService
{
    bool IsDarkMode { get; set; }
    string Language { get; set; }          // "fr" | "en"
    bool TruckReminders { get; set; }
    bool ReportUpdates { get; set; }
    void ApplyTheme();
    void ApplyLanguage();
}
