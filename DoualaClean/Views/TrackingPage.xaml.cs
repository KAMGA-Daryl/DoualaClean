namespace DoualaClean.Views;

public partial class TrackingPage : ContentPage
{
    private readonly TrackingViewModel _vm;

    public TrackingPage(TrackingViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.RefreshAsync();
    }

    private void OnReportTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: WasteReport report })
            _vm.OpenReportCommand.Execute(report);
    }
}
