using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DoualaClean.Services;

public sealed class ApiResult<T>
{
    public bool Ok { get; init; }
    public T? Data { get; init; }
    public string Error { get; init; } = "";
}

/// <summary>Client HTTP vers l'API DoualaClean.Api (JWT Bearer).</summary>
public sealed class ApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(100) };

    /// <summary>Émulateur Android : 10.0.2.2 = le PC hôte. Téléphone réel : saisir l'IP du PC (ex. http://192.168.1.20:5080).</summary>
    public static string DefaultUrl => DeviceInfo.Current.DeviceType == DeviceType.Virtual
        ? "http://10.0.2.2:5080"
        : "http://192.168.1.100:5080";

    public static string BaseUrl
    {
        get => Preferences.Default.Get("api_base_url", DefaultUrl).TrimEnd('/');
        set => Preferences.Default.Set("api_base_url", value.Trim().TrimEnd('/'));
    }

    public string? Token { get; set; }

    /// <summary>Teste si le serveur répond (utilisé par le bouton « Tester la connexion »).</summary>
    public static async Task<(bool Ok, string Message)> PingAsync()
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(6) };
            using var resp = await http.GetAsync(BaseUrl + "/api/health");
            return resp.IsSuccessStatusCode
                ? (true, $"Serveur joignable : {BaseUrl}")
                : (false, $"Le serveur répond avec une erreur ({(int)resp.StatusCode}) : {BaseUrl}");
        }
        catch (Exception)
        {
            return (false, $"Serveur injoignable : {BaseUrl}. Vérifiez que l'API est lancée, l'adresse saisie et le pare-feu Windows.");
        }
    }

    /// <summary>Déclenché quand le serveur refuse le jeton (session expirée).</summary>
    public event Action? Unauthorized;

    public Task<ApiResult<T>> GetAsync<T>(string path) => SendAsync<T>(HttpMethod.Get, path, null);

    public Task<ApiResult<T>> PostJsonAsync<T>(string path, object body) =>
        SendAsync<T>(HttpMethod.Post, path, new StringContent(JsonSerializer.Serialize(body, Json), Encoding.UTF8, "application/json"));

    public Task<ApiResult<T>> PostMultipartAsync<T>(string path, MultipartFormDataContent content) =>
        SendAsync<T>(HttpMethod.Post, path, content);

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string path, HttpContent? content)
    {
        try
        {
            using var req = new HttpRequestMessage(method, BaseUrl + path) { Content = content };
            var hadToken = !string.IsNullOrEmpty(Token);
            if (hadToken) req.Headers.Authorization = new("Bearer", Token);

            using var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                var data = string.IsNullOrWhiteSpace(body) ? default : JsonSerializer.Deserialize<T>(body, Json);
                return new ApiResult<T> { Ok = true, Data = data };
            }

            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized && hadToken)
            {
                Unauthorized?.Invoke();
                return new ApiResult<T> { Error = "Session expirée. Veuillez vous reconnecter." };
            }
            return new ApiResult<T> { Error = ExtractError(body, (int)resp.StatusCode) };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new ApiResult<T> { Error = $"Serveur injoignable ({BaseUrl}). Vérifiez que l'API est lancée et l'adresse du serveur." };
        }
        catch (Exception)
        {
            return new ApiResult<T> { Error = "Réponse inattendue du serveur." };
        }
    }

    private static string ExtractError(string body, int status)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var e) && e.GetString() is { Length: > 0 } msg) return msg;
        }
        catch { /* corps non JSON */ }
        return $"Erreur serveur ({status}).";
    }
}

// ---- DTOs échangés avec l'API ----
public record PointDto(int Points, string Reason, DateTime Date);
public record UserDto(int Id, string Email, string FullName, string Role, int GreenPoints, string Level, List<PointDto> History);
public record AuthResponse(string Token, UserDto User);
