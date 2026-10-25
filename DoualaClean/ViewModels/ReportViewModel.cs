namespace DoualaClean.ViewModels;

/// <summary>
/// Brouillon de signalement partagé par 4 écrans : Signaler un dépôt, Description, Photo, Localisation.
/// Enregistré en Singleton pour conserver les données entre les écrans.
/// </summary>
public partial class ReportViewModel : BaseViewModel
{
    private readonly IReportService _reports;
    public ReportViewModel(IReportService reports) => _reports = reports;

    public string[] WasteTypes { get; } =
    {
        "Déchets organiques", "Plastiques & emballages", "Verre & métaux", "Ordures ménagères", "Autre"
    };

    [ObservableProperty][NotifyPropertyChangedFor(nameof(DescriptionSummary))][NotifyPropertyChangedFor(nameof(CanSubmit))]
    private string description = "";

    [ObservableProperty][NotifyPropertyChangedFor(nameof(DescriptionSummary))]
    private string? selectedWasteType;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(PhotoSummary))][NotifyPropertyChangedFor(nameof(HasPhoto))][NotifyPropertyChangedFor(nameof(HasNoPhoto))]
    private string? photoPath;

    [ObservableProperty] private ImageSource? photoSource;
    [ObservableProperty] private string photoInfo = "";

    [ObservableProperty][NotifyPropertyChangedFor(nameof(LocationSummary))][NotifyPropertyChangedFor(nameof(CanSubmit))][NotifyPropertyChangedFor(nameof(GpsText))]
    private bool hasLocation;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(GpsText))] private double latitude;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(GpsText))] private double longitude;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(LocationSummary))][NotifyPropertyChangedFor(nameof(GpsText))] private string address = "";
    [ObservableProperty] private string quartier = "";

    public bool HasPhoto => !string.IsNullOrEmpty(PhotoPath);
    public bool HasNoPhoto => !HasPhoto;
    public bool CanSubmit => !string.IsNullOrWhiteSpace(Description) && HasLocation;

    public string DescriptionSummary =>
        string.IsNullOrWhiteSpace(Description) ? "Description du problème" : $"✏️ {Truncate(Description, 40)}";
    public string PhotoSummary => HasPhoto ? "📷 Photo jointe" : "Ajouter une photo";
    public string LocationSummary => HasLocation ? $"📍 {Truncate(Address, 38)}" : "Localisation (GPS)";
    public string GpsText => HasLocation
        ? $"Position GPS : {LocationHelper.FormatGps(Latitude, Longitude)}\nAdresse : {Address}"
        : "Position non définie";

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "…";

    // ---- Navigation ----
    [RelayCommand] private Task OpenDescriptionAsync() => Shell.Current.GoToAsync(nameof(DescriptionPage));
    [RelayCommand] private Task OpenPhotoAsync() => Shell.Current.GoToAsync(nameof(PhotoPage));
    [RelayCommand] private Task OpenLocationAsync() => Shell.Current.GoToAsync(nameof(LocationPage));
    [RelayCommand] private Task ConfirmAsync() => Shell.Current.GoToAsync("..");

    // ---- Photo ----
    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Shell.Current.DisplayAlert("Caméra indisponible", "Utilisez la galerie à la place.", "OK");
                return;
            }
            await SavePhotoAsync(await MediaPicker.Default.CapturePhotoAsync());
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Autorisation requise", "Autorisez l'accès à la caméra dans les paramètres.", "OK");
        }
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        try { await SavePhotoAsync(await MediaPicker.Default.PickPhotoAsync()); }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Autorisation requise", "Autorisez l'accès aux photos dans les paramètres.", "OK");
        }
    }

    [RelayCommand]
    private void RemovePhoto()
    {
        PhotoPath = null; PhotoSource = null; PhotoInfo = "";
    }

    private async Task SavePhotoAsync(FileResult? file)
    {
        if (file is null) return;
        var dir = Path.Combine(FileSystem.AppDataDirectory, "photos");
        Directory.CreateDirectory(dir);
        var name = $"depot_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(dir, name);
        await using (var src = await file.OpenReadAsync())
        await using (var dst = File.Create(path))
            await src.CopyToAsync(dst);

        PhotoPath = path;
        PhotoSource = ImageSource.FromFile(path);
        var mo = new FileInfo(path).Length / 1048576.0;
        PhotoInfo = $"Photo jointe : {name}\n(Statut : image chargée avec succès - {mo:0.0} Mo)";
    }

    // ---- GPS ----
    [RelayCommand]
    public async Task RefreshLocationAsync()
    {
        if (IsBusy) return;
        IsBusy = true; ErrorMessage = "";
        try
        {
            var r = await LocationHelper.GetCurrentAsync();
            if (r.Location is null) { ErrorMessage = r.Error; return; }
            Latitude = r.Location.Latitude;
            Longitude = r.Location.Longitude;
            Address = r.Address;
            Quartier = r.Quartier;
            HasLocation = true;
        }
        finally { IsBusy = false; }
    }

    // ---- Envoi ----
    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (IsBusy) return;
        if (!CanSubmit)
        {
            await Shell.Current.DisplayAlert("Signalement incomplet",
                "Ajoutez une description et votre localisation avant d'envoyer.", "OK");
            return;
        }
        IsBusy = true;
        try
        {
            await _reports.SubmitAsync(new ReportDraft
            {
                Description = Description.Trim(),
                WasteType = SelectedWasteType ?? "Autre",
                PhotoPath = PhotoPath,
                Latitude = Latitude,
                Longitude = Longitude,
                Address = Address,
                Quartier = Quartier
            });
            Reset();
            await Shell.Current.GoToAsync(nameof(ReportSuccessPage));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Envoi impossible", ex.Message, "OK");
        }
        finally { IsBusy = false; }
    }

    private void Reset()
    {
        Description = ""; SelectedWasteType = null; RemovePhoto();
        HasLocation = false; Latitude = 0; Longitude = 0; Address = ""; Quartier = "";
    }
}

public partial class ReportSuccessViewModel : BaseViewModel
{
    public string Message => "Merci pour votre contribution pour un Douala plus propre.";
    public string PointsText => "+50 Green Points gagnés !";

    [RelayCommand] private Task GoHomeAsync() => Shell.Current.GoToAsync("//HomePage");

    [RelayCommand]
    private async Task GoTrackingAsync()
    {
        await Shell.Current.GoToAsync("//HomePage");
        await Shell.Current.GoToAsync(nameof(TrackingPage));
    }
}
