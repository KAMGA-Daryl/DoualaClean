namespace DoualaClean;

public partial class App : Application
{
    private readonly AppShell _shell;

    public App(AppShell shell, ISettingsService settings)
    {
        InitializeComponent();
        _shell = shell;
        settings.ApplyLanguage();
        settings.ApplyTheme();
    }

    protected override Window CreateWindow(IActivationState? activationState) => new Window(_shell);
}
