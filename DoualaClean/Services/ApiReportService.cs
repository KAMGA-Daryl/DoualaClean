namespace DoualaClean.Services;

/// <summary>Signalements envoyés et lus via l'API serveur (photo en multipart).</summary>
public sealed class ApiReportService : IReportService
{
    private readonly IAuthService _auth;
    private readonly ApiClient _api;
    private List<WasteReport> _cache = new();

    public ApiReportService(IAuthService auth, ApiClient api)
    {
        _auth = auth;
        _api = api;
    }

    public async Task<IReadOnlyList<WasteReport>> GetMineAsync()
    {
        var r = await _api.GetAsync<List<WasteReport>>("/api/reports");
        if (!r.Ok) throw new InvalidOperationException(r.Error);
        _cache = r.Data ?? new();
        _cache.ForEach(FixPhotoUrl);
        return _cache;
    }

    public WasteReport? GetById(int id) => _cache.FirstOrDefault(r => r.Id == id);

    public async Task<WasteReport> SubmitAsync(ReportDraft draft)
    {
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        using var form = new MultipartFormDataContent
        {
            { new StringContent(draft.Description), "description" },
            { new StringContent(draft.WasteType), "wasteType" },
            { new StringContent(draft.Quartier), "quartier" },
            { new StringContent(draft.Address), "address" },
            { new StringContent(draft.Latitude.ToString("R", ci)), "latitude" },
            { new StringContent(draft.Longitude.ToString("R", ci)), "longitude" },
        };

        if (!string.IsNullOrEmpty(draft.PhotoPath) && File.Exists(draft.PhotoPath))
        {
            var photo = new StreamContent(File.OpenRead(draft.PhotoPath));
            var isPng = Path.GetExtension(draft.PhotoPath).Equals(".png", StringComparison.OrdinalIgnoreCase);
            photo.Headers.ContentType = new(isPng ? "image/png" : "image/jpeg");
            form.Add(photo, "photo", Path.GetFileName(draft.PhotoPath));
        }

        var r = await _api.PostMultipartAsync<WasteReport>("/api/reports", form);
        if (!r.Ok || r.Data is null) throw new InvalidOperationException(r.Error);

        FixPhotoUrl(r.Data);
        _cache.Insert(0, r.Data);
        _ = _auth.RefreshAsync();   // met à jour les Green Points
        return r.Data;
    }

    private static void FixPhotoUrl(WasteReport r)
    {
        if (!string.IsNullOrEmpty(r.PhotoUrl) && r.PhotoUrl.StartsWith('/'))
            r.PhotoUrl = ApiClient.BaseUrl + r.PhotoUrl;
    }
}
