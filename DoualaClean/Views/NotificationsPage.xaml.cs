namespace DoualaClean.Views;

public partial class NotificationsPage : ContentPage
{
    private readonly SettingsViewModel _vm;

    public NotificationsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }
}
