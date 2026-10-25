using System.Globalization;

namespace DoualaClean.Services;

public sealed class SettingsService : ISettingsService
{
    public bool IsDarkMode
    {
        get => Preferences.Default.Get(nameof(IsDarkMode), false);
        set { Preferences.Default.Set(nameof(IsDarkMode), value); ApplyTheme(); }
    }

    public string Language
    {
        get => Preferences.Default.Get(nameof(Language), "fr");
        set { Preferences.Default.Set(nameof(Language), value); ApplyLanguage(); }
    }

    public bool TruckReminders
    {
        get => Preferences.Default.Get(nameof(TruckReminders), true);
        set => Preferences.Default.Set(nameof(TruckReminders), value);
    }

    public bool ReportUpdates
    {
        get => Preferences.Default.Get(nameof(ReportUpdates), true);
        set => Preferences.Default.Set(nameof(ReportUpdates), value);
    }

    public void ApplyTheme()
    {
        if (Application.Current is { } app)
            app.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
    }

    public void ApplyLanguage()
    {
        var culture = new CultureInfo(Language);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}
