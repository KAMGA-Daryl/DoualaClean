namespace DoualaClean.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    public LoginViewModel(IAuthService auth) => _auth = auth;

    [ObservableProperty] private string email = "";
    [ObservableProperty] private string password = "";
    [ObservableProperty] private string serverUrl = AppConfig.UseServer ? ApiClient.BaseUrl : "";

    public bool ShowServerLink => AppConfig.UseServer;
    public bool ShowDemoHint => !AppConfig.UseServer;
    public string DemoHint => $"Compte de démonstration : {LocalAuthService.DemoEmail}  ·  {LocalAuthService.DemoPassword}";

    [ObservableProperty] private string serverStatus = "";

    [RelayCommand]
    private async Task TestServerAsync()
    {
        ServerStatus = "Test en cours…";
        var (ok, message) = await ApiClient.PingAsync();
        ServerStatus = (ok ? "✔ " : "✖ ") + message;
    }

    [RelayCommand]
    private async Task ConfigureServerAsync()
    {
        var url = await Shell.Current.DisplayPromptAsync("Adresse du serveur",
            "Émulateur : http://10.0.2.2:5080\nTéléphone : http://IP-DU-PC:5080",
            "OK", "Annuler", initialValue: ApiClient.BaseUrl, keyboard: Keyboard.Url);
        if (string.IsNullOrWhiteSpace(url)) return;
        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            await Shell.Current.DisplayAlert("Adresse invalide", "Exemple : http://192.168.1.20:5080", "OK");
            return;
        }
        ApiClient.BaseUrl = url;
        ServerUrl = ApiClient.BaseUrl;
        ServerStatus = "";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;
        ErrorMessage = "";
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Veuillez renseigner votre email et votre mot de passe.";
            return;
        }
        IsBusy = true;
        try
        {
            var result = await _auth.LoginAsync(Email.Trim(), Password);
            if (!result.Success) { ErrorMessage = result.Error; return; }
            Password = "";
            await Shell.Current.GoToAsync("//HomePage");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand] private Task GoRegisterAsync() => Shell.Current.GoToAsync(nameof(RegisterPage));
    [RelayCommand] private Task GoForgotAsync() => Shell.Current.GoToAsync(nameof(ForgotPasswordPage));
}

public partial class RegisterViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    public RegisterViewModel(IAuthService auth) => _auth = auth;

    [ObservableProperty] private string email = "";
    [ObservableProperty] private string fullName = "";
    [ObservableProperty] private string password = "";

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy) return;
        ErrorMessage = "";
        IsBusy = true;
        try
        {
            var result = await _auth.RegisterAsync(Email, FullName, Password);
            if (!result.Success) { ErrorMessage = result.Error; return; }
            Password = "";
            await Shell.Current.GoToAsync("//HomePage");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand] private Task GoLoginAsync() => Shell.Current.GoToAsync("..");
}

public partial class ForgotPasswordViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    public ForgotPasswordViewModel(IAuthService auth) => _auth = auth;

    [ObservableProperty] private string email = "";

    [RelayCommand]
    private async Task SendAsync()
    {
        if (IsBusy) return;
        ErrorMessage = "";
        IsBusy = true;
        try
        {
            var result = await _auth.SendRecoveryAsync(Email);
            if (!result.Success) { ErrorMessage = result.Error; return; }
            await Shell.Current.DisplayAlert("Email envoyé",
                "Si un compte existe avec cette adresse, vous recevrez un lien de réinitialisation.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task GoogleAsync()
    {
        if (IsBusy) return;
        ErrorMessage = "";
        IsBusy = true;
        try
        {
            var result = await _auth.LoginWithGoogleAsync();
            if (!result.Success) { ErrorMessage = result.Error; return; }
            await Shell.Current.GoToAsync("//HomePage");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand] private Task GoRegisterAsync() => Shell.Current.GoToAsync(nameof(RegisterPage));
}
