namespace DoualaClean.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    public ProfileViewModel(IAuthService auth) => _auth = auth;

    [ObservableProperty] private string fullName = "";
    [ObservableProperty] private string initials = "";
    [ObservableProperty] private int greenPoints;

    public void Refresh()
    {
        var u = _auth.CurrentUser;
        FullName = u?.FullName ?? "";
        Initials = u?.Initials ?? "";
        GreenPoints = u?.GreenPoints ?? 0;
    }

    public async Task RefreshAsync()
    {
        Refresh();
        await _auth.RefreshAsync();
        Refresh();
    }

    [RelayCommand] private Task GoPointsAsync() => Shell.Current.GoToAsync(nameof(GreenPointsPage));
    [RelayCommand] private Task GoReportsAsync() => Shell.Current.GoToAsync(nameof(TrackingPage));
    [RelayCommand] private Task GoNotificationsAsync() => Shell.Current.GoToAsync(nameof(NotificationsPage));
    [RelayCommand] private Task GoSettingsAsync() => Shell.Current.GoToAsync(nameof(SettingsPage));

    [RelayCommand]
    private async Task LogoutAsync()
    {
        if (!await Shell.Current.DisplayAlert("Déconnexion", "Voulez-vous vraiment vous déconnecter ?", "Oui", "Annuler"))
            return;
        _auth.Logout();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}

public partial class GreenPointsViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    public GreenPointsViewModel(IAuthService auth) => _auth = auth;

    [ObservableProperty] private int points;
    [ObservableProperty] private string level = "";
    [ObservableProperty] private IReadOnlyList<PointsEntry> history = Array.Empty<PointsEntry>();

    public void Refresh()
    {
        var u = _auth.CurrentUser;
        Points = u?.GreenPoints ?? 0;
        Level = u?.Level ?? "";
        History = u?.PointsHistory.OrderByDescending(p => p.Date).ToList() ?? new();
    }

    public async Task RefreshAsync()
    {
        Refresh();
        await _auth.RefreshAsync();
        Refresh();
    }
}

public partial class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsService _settings;
    private readonly IAuthService _auth;

    public SettingsViewModel(ISettingsService settings, IAuthService auth)
    {
        _settings = settings;
        _auth = auth;
        FullName = auth.CurrentUser?.FullName ?? "";
        IsDarkMode = settings.IsDarkMode;
        Language = settings.Language;
        TruckReminders = settings.TruckReminders;
        ReportUpdates = settings.ReportUpdates;
    }

    [ObservableProperty] private string fullName = "";

    [ObservableProperty][NotifyPropertyChangedFor(nameof(ThemeIcon))][NotifyPropertyChangedFor(nameof(ThemeHint))]
    private bool isDarkMode;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(FrenchText))][NotifyPropertyChangedFor(nameof(EnglishText))]
    private string language = "fr";

    [ObservableProperty] private bool truckReminders;
    [ObservableProperty] private bool reportUpdates;

    // Maquette : 🌙 en mode sombre (clic -> mode clair), ☀️ en mode clair (clic -> mode sombre)
    public string ThemeIcon => IsDarkMode ? "🌙" : "☀️";
    public string ThemeHint => IsDarkMode ? "Clique sur l'icône pour activer le mode clair"
                                          : "Clique sur l'icône pour activer le mode sombre";
    public string FrenchText => Language == "fr" ? "Français ✓" : "Français";
    public string EnglishText => Language == "en" ? "English ✓" : "English";

    partial void OnIsDarkModeChanged(bool value) => _settings.IsDarkMode = value;
    partial void OnLanguageChanged(string value) => _settings.Language = value;
    partial void OnTruckRemindersChanged(bool value) => _settings.TruckReminders = value;
    partial void OnReportUpdatesChanged(bool value) => _settings.ReportUpdates = value;

    [RelayCommand] private void ToggleTheme() => IsDarkMode = !IsDarkMode;
    [RelayCommand] private void SetLanguage(string lang) => Language = lang;
    [RelayCommand] private Task GoLanguageAsync() => Shell.Current.GoToAsync(nameof(LanguagePage));
    [RelayCommand] private Task GoNotificationsAsync() => Shell.Current.GoToAsync(nameof(NotificationsPage));
}
