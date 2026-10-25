namespace DoualaClean.ViewModels;

public partial class SortingGuideViewModel : BaseViewModel
{
    public IReadOnlyList<SortingCategory> Categories { get; }
    public SortingGuideViewModel(ISortingGuideService guide) => Categories = guide.GetAll();

    [RelayCommand] private Task OpenCategoryAsync(SortingCategory c) => Shell.Current.GoToAsync($"{nameof(SortingDetailPage)}?key={c.Key}");
}

public partial class SortingDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ISortingGuideService _guide;
    private readonly IAuthService _auth;

    public SortingDetailViewModel(ISortingGuideService guide, IAuthService auth)
    {
        _guide = guide;
        _auth = auth;
    }

    [ObservableProperty] private SortingCategory? category;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("key", out var v))
        {
            Category = _guide.GetByKey(v?.ToString() ?? "");
            // +50 pts la première fois qu'on consulte le guide (comme dans l'écran Mes Green Points)
            _ = _auth.AwardOnceAsync("guide");
        }
    }
}
