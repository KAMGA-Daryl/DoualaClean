namespace DoualaClean.Views;

public partial class LanguagePage : ContentPage
{
    private readonly SettingsViewModel _vm;

    public LanguagePage(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }
}
