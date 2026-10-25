namespace DoualaClean.ViewModels;

public partial class TrackingViewModel : BaseViewModel
{
    private readonly IReportService _reports;
    public TrackingViewModel(IReportService reports) => _reports = reports;

    [ObservableProperty] private IReadOnlyList<WasteReport> inProgress = Array.Empty<WasteReport>();
    [ObservableProperty] private IReadOnlyList<WasteReport> resolved = Array.Empty<WasteReport>();
    [ObservableProperty] private bool isEmpty;

    public async Task RefreshAsync()
    {
        if (IsBusy) return;
        IsBusy = true; ErrorMessage = "";
        try
        {
            var all = await _reports.GetMineAsync();
            InProgress = all.Where(r => !r.IsResolved).ToList();
            Resolved = all.Where(r => r.IsResolved).ToList();
            IsEmpty = all.Count == 0;
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand] private Task OpenReportAsync(WasteReport r) => Shell.Current.GoToAsync($"{nameof(ReportDetailPage)}?id={r.Id}");
}

public partial class ReportDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IReportService _reports;
    public ReportDetailViewModel(IReportService reports) => _reports = reports;

    [ObservableProperty] private WasteReport? report;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var v) && int.TryParse(v?.ToString(), out var id))
            Report = _reports.GetById(id);
    }
}
