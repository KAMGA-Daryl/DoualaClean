using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.Json;

namespace DoualaClean.Services;

/// <summary>
/// Mode AUTONOME : comptes stockés sur le téléphone (Preferences), mots de passe hachés (PBKDF2).
/// Un compte de démonstration reprend les maquettes (Daryl Kamga, 150 points).
/// </summary>
public sealed class LocalAuthService : IAuthService
{
    private const string UsersKey = "local_users_v1";
    private const string SessionKey = "local_session_email";
    public const string DemoEmail = "daryl.kamga@example.com";
    public const string DemoPassword = "Douala2026";

    private readonly List<AppUser> _users;

    public AppUser? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser is not null;
    public event Action? UserChanged;

    public LocalAuthService()
    {
        var json = Preferences.Default.Get(UsersKey, "");
        _users = string.IsNullOrEmpty(json)
            ? new List<AppUser>()
            : JsonSerializer.Deserialize<List<AppUser>>(json) ?? new List<AppUser>();

        if (_users.Count == 0) SeedDemoUser();

        var session = Preferences.Default.Get(SessionKey, "");
        if (!string.IsNullOrEmpty(session))
            CurrentUser = _users.FirstOrDefault(u => u.Email.Equals(session, StringComparison.OrdinalIgnoreCase));
    }

    private void SeedDemoUser()
    {
        _users.Add(new AppUser
        {
            Email = DemoEmail,
            FullName = "Daryl Kamga",
            PasswordHash = Hash(DemoPassword),
            Awards = new List<string> { "guide" },
            PointsHistory =
            {
                new PointsEntry { Points = 50, Reason = "Signalement validé à Akwa", Date = DateTime.Now.AddDays(-9) },
                new PointsEntry { Points = 50, Reason = "Guide de tri consulté", Date = DateTime.Now.AddDays(-8) },
                new PointsEntry { Points = 50, Reason = "Dépôt nettoyé par les services", Date = DateTime.Now.AddDays(-5) },
            }
        });
        Save();
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        await Task.Delay(300);
        var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (user is null || !Verify(password, user.PasswordHash))
            return new AuthResult(false, "Email ou mot de passe incorrect.");
        StartSession(user);
        return new AuthResult(true);
    }

    public async Task<AuthResult> RegisterAsync(string email, string fullName, string password)
    {
        await Task.Delay(300);
        email = email.Trim();
        if (!IsValidEmail(email)) return new AuthResult(false, "Adresse email invalide.");
        if (string.IsNullOrWhiteSpace(fullName)) return new AuthResult(false, "Le nom complet est obligatoire.");
        if (password.Length < 6) return new AuthResult(false, "Le mot de passe doit contenir au moins 6 caractères.");
        if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            return new AuthResult(false, "Un compte existe déjà avec cet email.");

        var user = new AppUser { Email = email, FullName = fullName.Trim(), PasswordHash = Hash(password) };
        _users.Add(user);
        Save();
        StartSession(user);
        return new AuthResult(true);
    }

    public async Task<AuthResult> SendRecoveryAsync(string email)
    {
        await Task.Delay(400);
        // Mode autonome : aucun serveur d'email. Message neutre côté interface.
        return IsValidEmail(email.Trim()) ? new AuthResult(true) : new AuthResult(false, "Adresse email invalide.");
    }

    public async Task<AuthResult> LoginWithGoogleAsync()
    {
        var (idToken, error) = await GoogleSignIn.GetIdTokenAsync();
        if (idToken is null) return new AuthResult(false, error);

        // Mode autonome : on lit l'email et le nom dans le jeton Google (pas de serveur pour le vérifier).
        var (email, name) = GoogleSignIn.ReadProfile(idToken);
        if (!IsValidEmail(email)) return new AuthResult(false, "Le compte Google n'a pas fourni d'email.");

        var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            user = new AppUser
            {
                Email = email,
                FullName = string.IsNullOrWhiteSpace(name) ? email.Split('@')[0] : name,
                PasswordHash = Hash(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)))
            };
            _users.Add(user);
            Save();
        }
        StartSession(user);
        return new AuthResult(true);
    }

    public Task RefreshAsync() => Task.CompletedTask;

    public void Logout()
    {
        CurrentUser = null;
        Preferences.Default.Remove(SessionKey);
        UserChanged?.Invoke();
    }

    public Task AwardOnceAsync(string key)
    {
        if (CurrentUser is null || key != "guide" || CurrentUser.Awards.Contains(key)) return Task.CompletedTask;
        CurrentUser.Awards.Add(key);
        AddPoints(50, "Guide de tri consulté");
        return Task.CompletedTask;
    }

    /// <summary>Utilisé par LocalReportService (+50 par signalement).</summary>
    public void AddPoints(int points, string reason)
    {
        if (CurrentUser is null) return;
        CurrentUser.PointsHistory.Insert(0, new PointsEntry { Points = points, Reason = reason });
        Save();
        UserChanged?.Invoke();
    }

    private void StartSession(AppUser user)
    {
        CurrentUser = user;
        Preferences.Default.Set(SessionKey, user.Email);
        UserChanged?.Invoke();
    }

    private void Save() => Preferences.Default.Set(UsersKey, JsonSerializer.Serialize(_users));

    private static bool IsValidEmail(string email)
    {
        try { return new MailAddress(email).Address == email; }
        catch { return false; }
    }

    private static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool Verify(string password, string stored)
    {
        var parts = stored.Split(':');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}
