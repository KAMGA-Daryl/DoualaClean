namespace DoualaClean.Views;

public partial class ReportDetailPage : ContentPage
{
    private readonly ReportDetailViewModel _vm;

    public ReportDetailPage(ReportDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }
}
