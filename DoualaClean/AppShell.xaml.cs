namespace DoualaClean;

public partial class AppShell : Shell
{
    public AppShell(IAuthService auth)
    {
        InitializeComponent();

        // Pages empilées (navigation "push"), le bouton < revient en arrière via GoToAsync("..")
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
        Routing.RegisterRoute(nameof(ReportPage), typeof(ReportPage));
        Routing.RegisterRoute(nameof(DescriptionPage), typeof(DescriptionPage));
        Routing.RegisterRoute(nameof(PhotoPage), typeof(PhotoPage));
        Routing.RegisterRoute(nameof(LocationPage), typeof(LocationPage));
        Routing.RegisterRoute(nameof(ReportSuccessPage), typeof(ReportSuccessPage));
        Routing.RegisterRoute(nameof(BinsPage), typeof(BinsPage));
        Routing.RegisterRoute(nameof(LocatedPage), typeof(LocatedPage));
        Routing.RegisterRoute(nameof(BinDetailPage), typeof(BinDetailPage));
        Routing.RegisterRoute(nameof(SortingGuidePage), typeof(SortingGuidePage));
        Routing.RegisterRoute(nameof(SortingDetailPage), typeof(SortingDetailPage));
        Routing.RegisterRoute(nameof(TrackingPage), typeof(TrackingPage));
        Routing.RegisterRoute(nameof(ReportDetailPage), typeof(ReportDetailPage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
        Routing.RegisterRoute(nameof(GreenPointsPage), typeof(GreenPointsPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(LanguagePage), typeof(LanguagePage));
        Routing.RegisterRoute(nameof(NotificationsPage), typeof(NotificationsPage));

        // Session déjà ouverte -> on démarre sur l'accueil
        if (auth.IsLoggedIn) CurrentItem = HomeItem;
    }
}
