namespace DoualaClean.Views;

public partial class ReportSuccessPage : ContentPage
{
    private readonly ReportSuccessViewModel _vm;

    public ReportSuccessPage(ReportSuccessViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }
}
