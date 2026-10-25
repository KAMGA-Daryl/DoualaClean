using System.ComponentModel;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace DoualaClean.Views;

public partial class LocationPage : ContentPage
{
    private readonly ReportViewModel _vm;

    public LocationPage(ReportViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _vm.PropertyChanged += OnVmPropertyChanged;
        UpdateMap();
        if (!_vm.HasLocation) await _vm.RefreshLocationAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.PropertyChanged -= OnVmPropertyChanged;
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ReportViewModel.HasLocation) or nameof(ReportViewModel.Latitude))
            MainThread.BeginInvokeOnMainThread(UpdateMap);
    }

    private void UpdateMap()
    {
        if (!_vm.HasLocation) return;
        var position = new Location(_vm.Latitude, _vm.Longitude);
        LocationMap.Pins.Clear();
        LocationMap.Pins.Add(new Pin { Label = "Dépôt signalé", Address = _vm.Address, Location = position, Type = PinType.Place });
        LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMeters(300)));
    }
}
