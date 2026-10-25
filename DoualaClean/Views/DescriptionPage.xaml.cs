namespace DoualaClean.Views;

public partial class DescriptionPage : ContentPage
{
    private readonly ReportViewModel _vm;

    public DescriptionPage(ReportViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }
}
