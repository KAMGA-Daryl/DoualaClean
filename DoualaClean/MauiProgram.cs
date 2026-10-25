using Microsoft.Extensions.Logging;

namespace DoualaClean;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps();

        // Services (backend local)
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        if (AppConfig.UseServer)
        {
            // Application + serveur DoualaClean.Api
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<IAuthService, ApiAuthService>();
            builder.Services.AddSingleton<IReportService, ApiReportService>();
        }
        else
        {
            // Application autonome (frontend seul) : données stockées sur le téléphone
            builder.Services.AddSingleton<LocalAuthService>();
            builder.Services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<LocalAuthService>());
            builder.Services.AddSingleton<IReportService, LocalReportService>();
        }
        builder.Services.AddSingleton<IBinService, BinService>();
        builder.Services.AddSingleton<ISortingGuideService, SortingGuideService>();

        builder.Services.AddSingleton<AppShell>();

        // ViewModels (ReportViewModel est partagé entre les 4 écrans du signalement)
        builder.Services.AddSingleton<ReportViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ReportSuccessViewModel>();
        builder.Services.AddTransient<BinsViewModel>();
        builder.Services.AddTransient<BinDetailViewModel>();
        builder.Services.AddTransient<LocatedViewModel>();
        builder.Services.AddTransient<SortingGuideViewModel>();
        builder.Services.AddTransient<SortingDetailViewModel>();
        builder.Services.AddTransient<TrackingViewModel>();
        builder.Services.AddTransient<ReportDetailViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<GreenPointsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<ReportPage>();
        builder.Services.AddTransient<DescriptionPage>();
        builder.Services.AddTransient<PhotoPage>();
        builder.Services.AddTransient<LocationPage>();
        builder.Services.AddTransient<ReportSuccessPage>();
        builder.Services.AddTransient<BinsPage>();
        builder.Services.AddTransient<LocatedPage>();
        builder.Services.AddTransient<BinDetailPage>();
        builder.Services.AddTransient<SortingGuidePage>();
        builder.Services.AddTransient<SortingDetailPage>();
        builder.Services.AddTransient<TrackingPage>();
        builder.Services.AddTransient<ReportDetailPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<GreenPointsPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<LanguagePage>();
        builder.Services.AddTransient<NotificationsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
