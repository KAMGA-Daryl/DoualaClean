namespace DoualaClean.Controls;

public partial class BackButton : ContentView
{
    public BackButton() => InitializeComponent();

    private async void OnClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}
