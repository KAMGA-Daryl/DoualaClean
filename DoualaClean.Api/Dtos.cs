using Microsoft.EntityFrameworkCore;

namespace DoualaClean.Api;

public record RegisterRequest(string? Email, string? FullName, string? Password);
public record LoginRequest(string? Email, string? Password);
public record ForgotRequest(string? Email);
public record GoogleRequest(string? IdToken);
public record StatusRequest(string? Status);

public record PointDto(int Points, string Reason, DateTime Date);
public record UserDto(int Id, string Email, string FullName, string Role, int GreenPoints, string Level, List<PointDto> History);
public record AuthResponse(string Token, UserDto User);
public record ReportDto(int Id, string Quartier, string Address, string WasteType, string Description,
                        string? PhotoUrl, double Latitude, double Longitude, string Status,
                        DateTime CreatedAt, DateTime? ResolvedAt, int PointsEarned,
                        string? UserName, string? UserEmail);

public static class Mapper
{
    // SQLite renvoie des dates "Unspecified" : on force UTC pour que le JSON porte le suffixe Z
    public static DateTime Utc(DateTime d) => DateTime.SpecifyKind(d, DateTimeKind.Utc);

    public static string Level(int points) => points switch
    {
        < 100 => "Éco-débutant",
        < 300 => "Éco-citoyen actif",
        _ => "Éco-champion"
    };

    public static ReportDto ToDto(Report r, User? u = null) => new(
        r.Id, r.Quartier, r.Address, r.WasteType, r.Description,
        r.PhotoFileName is null ? null : $"/photos/{r.PhotoFileName}",
        r.Latitude, r.Longitude, r.Status.ToString(),
        Utc(r.CreatedAt), r.ResolvedAt is null ? null : Utc(r.ResolvedAt.Value), r.PointsEarned,
        u?.FullName, u?.Email);

    public static async Task<UserDto> ToUserDto(AppDb db, User u)
    {
        var history = await db.Points.Where(p => p.UserId == u.Id).OrderByDescending(p => p.Date).ToListAsync();
        var total = history.Sum(p => p.Points);
        return new UserDto(u.Id, u.Email, u.FullName, u.Role, total, Level(total),
            history.Select(p => new PointDto(p.Points, p.Reason, Utc(p.Date))).ToList());
    }
}
