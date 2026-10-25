namespace DoualaClean.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    public HomeViewModel(IAuthService auth)
    {
        _auth = auth;
        Refresh();
    }

    [ObservableProperty] private string userName = "";
    [ObservableProperty] private int greenPoints;

    public void Refresh()
    {
        UserName = _auth.CurrentUser?.FullName ?? "";
        GreenPoints = _auth.CurrentUser?.GreenPoints ?? 0;
    }

    public async Task RefreshAsync()
    {
        Refresh();
        await _auth.RefreshAsync();
        Refresh();
    }

    [RelayCommand] private Task GoReportAsync() => Shell.Current.GoToAsync(nameof(ReportPage));
    [RelayCommand] private Task GoBinsAsync() => Shell.Current.GoToAsync(nameof(BinsPage));
    [RelayCommand] private Task GoGuideAsync() => Shell.Current.GoToAsync(nameof(SortingGuidePage));
    [RelayCommand] private Task GoTrackingAsync() => Shell.Current.GoToAsync(nameof(TrackingPage));
    [RelayCommand] private Task GoProfileAsync() => Shell.Current.GoToAsync(nameof(ProfilePage));
}
