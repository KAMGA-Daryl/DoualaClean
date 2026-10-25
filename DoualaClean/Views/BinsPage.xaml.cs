namespace DoualaClean.Views;

public partial class BinsPage : ContentPage
{
    private readonly BinsViewModel _vm;

    public BinsPage(BinsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    private void OnBinTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: CollectionBin bin })
            _vm.OpenBinCommand.Execute(bin);
    }
}
