using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DoualaClean.Services;

/// <summary>Authentification via l'API serveur. Le jeton JWT et le profil sont mis en cache pour démarrer hors-ligne.</summary>
public sealed class ApiAuthService : IAuthService
{
    private const string TokenKey = "api_token";
    private const string UserKey = "cached_user";
    private readonly ApiClient _api;

    public AppUser? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser is not null && !string.IsNullOrEmpty(_api.Token);
    public event Action? UserChanged;

    public ApiAuthService(ApiClient api)
    {
        _api = api;
        _api.Unauthorized += OnUnauthorized;

        var token = Preferences.Default.Get(TokenKey, "");
        var cached = Preferences.Default.Get(UserKey, "");
        if (token.Length > 0 && cached.Length > 0)
        {
            try
            {
                CurrentUser = JsonSerializer.Deserialize<AppUser>(cached);
                _api.Token = token;
            }
            catch { ClearSession(); }
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var r = await _api.PostJsonAsync<AuthResponse>("/api/auth/login", new { email, password });
        return HandleAuth(r);
    }

    public async Task<AuthResult> RegisterAsync(string email, string fullName, string password)
    {
        var r = await _api.PostJsonAsync<AuthResponse>("/api/auth/register", new { email, fullName, password });
        return HandleAuth(r);
    }

    public async Task<AuthResult> SendRecoveryAsync(string email)
    {
        var r = await _api.PostJsonAsync<JsonElement>("/api/auth/forgot", new { email });
        return r.Ok ? new AuthResult(true) : new AuthResult(false, r.Error);
    }

    public async Task<AuthResult> LoginWithGoogleAsync()
    {
        var (idToken, error) = await GoogleSignIn.GetIdTokenAsync();
        if (idToken is null) return new AuthResult(false, error);

        // Le serveur vérifie le jeton auprès de Google et ouvre (ou crée) le compte
        var r = await _api.PostJsonAsync<AuthResponse>("/api/auth/google", new { idToken });
        return HandleAuth(r);
    }

    public async Task RefreshAsync()
    {
        if (string.IsNullOrEmpty(_api.Token)) return;
        var r = await _api.GetAsync<UserDto>("/api/me");
        if (r.Ok && r.Data is not null) SetUser(r.Data);
    }

    public async Task AwardOnceAsync(string key)
    {
        if (string.IsNullOrEmpty(_api.Token)) return;
        var r = await _api.PostJsonAsync<JsonElement>($"/api/me/awards/{key}", new { });
        if (r.Ok) await RefreshAsync();
    }

    public void Logout()
    {
        ClearSession();
        UserChanged?.Invoke();
    }

    private AuthResult HandleAuth(ApiResult<AuthResponse> r)
    {
        if (!r.Ok || r.Data is null) return new AuthResult(false, r.Error);
        _api.Token = r.Data.Token;
        Preferences.Default.Set(TokenKey, r.Data.Token);
        SetUser(r.Data.User);
        return new AuthResult(true);
    }

    private void SetUser(UserDto dto)
    {
        CurrentUser = new AppUser
        {
            Email = dto.Email,
            FullName = dto.FullName,
            PointsHistory = dto.History.Select(h => new PointsEntry { Points = h.Points, Reason = h.Reason, Date = h.Date.ToLocalTime() }).ToList()
        };
        Preferences.Default.Set(UserKey, JsonSerializer.Serialize(CurrentUser));
        UserChanged?.Invoke();
    }

    private void ClearSession()
    {
        CurrentUser = null;
        _api.Token = null;
        Preferences.Default.Remove(TokenKey);
        Preferences.Default.Remove(UserKey);
    }

    private void OnUnauthorized()
    {
        Logout();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try { await Shell.Current.GoToAsync("//LoginPage"); } catch { /* shell pas encore prêt */ }
        });
    }
}
