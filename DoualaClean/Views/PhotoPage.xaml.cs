namespace DoualaClean.Views;

public partial class PhotoPage : ContentPage
{
    private readonly ReportViewModel _vm;

    public PhotoPage(ReportViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }
}
