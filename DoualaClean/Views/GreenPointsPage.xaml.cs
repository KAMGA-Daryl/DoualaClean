namespace DoualaClean.Views;

public partial class GreenPointsPage : ContentPage
{
    private readonly GreenPointsViewModel _vm;

    public GreenPointsPage(GreenPointsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.RefreshAsync();
    }
}
