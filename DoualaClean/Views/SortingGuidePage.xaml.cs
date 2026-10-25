namespace DoualaClean.Views;

public partial class SortingGuidePage : ContentPage
{
    private readonly SortingGuideViewModel _vm;

    public SortingGuidePage(SortingGuideViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    private void OnCategoryTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: SortingCategory cat })
            _vm.OpenCategoryCommand.Execute(cat);
    }
}
