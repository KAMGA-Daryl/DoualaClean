namespace DoualaClean.Views;

public partial class ReportPage : ContentPage
{
    private readonly ReportViewModel _vm;

    public ReportPage(ReportViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }
}
