namespace DoualaClean.Views;

public partial class BinDetailPage : ContentPage
{
    private readonly BinDetailViewModel _vm;

    public BinDetailPage(BinDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }
}
