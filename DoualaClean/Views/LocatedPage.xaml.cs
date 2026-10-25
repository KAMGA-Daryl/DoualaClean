using System.ComponentModel;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace DoualaClean.Views;

public partial class LocatedPage : ContentPage
{
    private readonly LocatedViewModel _vm;

    public LocatedPage(LocatedViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        AddBinPins();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _vm.PropertyChanged += OnVmPropertyChanged;
        await _vm.LocateAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.PropertyChanged -= OnVmPropertyChanged;
    }

    private void AddBinPins()
    {
        foreach (var bin in _vm.Bins)
        {
            var pin = new Pin
            {
                Label = bin.Name,
                Address = $"{bin.WasteType} - rempli à {bin.FillPercent}%",
                Location = new Location(bin.Latitude, bin.Longitude),
                Type = PinType.Place,
                AutomationId = bin.Id.ToString()
            };
            // Toucher la bulle d'info ouvre l'écran "Détails du bac"
            pin.InfoWindowClicked += async (_, _) => await Shell.Current.GoToAsync($"{nameof(BinDetailPage)}?id={bin.Id}");
            BinsMap.Pins.Add(pin);
        }

        // Vue par défaut : centre de Douala, tant que la position n'est pas connue
        BinsMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(4.0511, 9.7679), Distance.FromKilometers(5)));
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LocatedViewModel.HasLocation) && _vm.HasLocation)
            MainThread.BeginInvokeOnMainThread(() =>
                BinsMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(_vm.Latitude, _vm.Longitude), Distance.FromKilometers(2))));
    }
}
