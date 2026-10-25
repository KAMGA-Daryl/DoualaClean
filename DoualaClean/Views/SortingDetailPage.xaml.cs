namespace DoualaClean.Views;

public partial class SortingDetailPage : ContentPage
{
    private readonly SortingDetailViewModel _vm;

    public SortingDetailPage(SortingDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }
}
