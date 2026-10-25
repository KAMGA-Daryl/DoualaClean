namespace DoualaClean.ViewModels;

public partial class BinsViewModel : BaseViewModel
{
    public IReadOnlyList<CollectionBin> Bins { get; }
    public BinsViewModel(IBinService bins) => Bins = bins.GetAll();

    [RelayCommand] private Task OpenBinAsync(CollectionBin bin) => Shell.Current.GoToAsync($"{nameof(BinDetailPage)}?id={bin.Id}");
    [RelayCommand] private Task CenterOnMeAsync() => Shell.Current.GoToAsync(nameof(LocatedPage));
}

public partial class BinDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IBinService _bins;
    public BinDetailViewModel(IBinService bins) => _bins = bins;

    [ObservableProperty] private CollectionBin? bin;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var v) && int.TryParse(v?.ToString(), out var id))
            Bin = _bins.GetById(id);
    }
}

public partial class LocatedViewModel : BaseViewModel
{
    public IReadOnlyList<CollectionBin> Bins { get; }
    public LocatedViewModel(IBinService bins) => Bins = bins.GetAll();

    [ObservableProperty] private double latitude;
    [ObservableProperty] private double longitude;
    [ObservableProperty] private bool hasLocation;
    [ObservableProperty] private string positionText = "Recherche de votre position…";

    [RelayCommand]
    public async Task LocateAsync()
    {
        if (IsBusy) return;
        IsBusy = true; ErrorMessage = "";
        try
        {
            var r = await LocationHelper.GetCurrentAsync();
            if (r.Location is null) { ErrorMessage = r.Error; PositionText = "Position indisponible"; return; }
            Latitude = r.Location.Latitude;
            Longitude = r.Location.Longitude;
            PositionText = $"📍 Position actuelle : {r.Address}";
            HasLocation = true;
        }
        finally { IsBusy = false; }
    }
}
