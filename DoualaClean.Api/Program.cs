using System.Globalization;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using Google.Apis.Auth;
using System.Text;
using DoualaClean.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=doualaclean.db"));

var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
builder.Services.AddSingleton(new TokenService(signingKey, jwt["Issuer"]!));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwt["Issuer"],
        ValidateAudience = false,
        ValidateIssuerSigningKey = true, IssuerSigningKey = signingKey,
        ValidateLifetime = true, ClockSkew = TimeSpan.FromMinutes(1)
    };
    // Messages dans la fenêtre du serveur pour comprendre un refus d'accès
    o.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
        {
            Console.WriteLine("[JWT] Jeton refusé : " + ctx.Exception.Message);
            return Task.CompletedTask;
        },
        OnForbidden = ctx =>
        {
            Console.WriteLine("[JWT] Accès interdit (rôle Admin absent du jeton) : " + ctx.Request.Path);
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

// ---------- Base de données + compte admin ----------
var uploads = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploads);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.EnsureCreated();
    var adminEmail = (app.Configuration["Admin:Email"] ?? "admin@doualaclean.local").ToLowerInvariant();
    if (!db.Users.Any(u => u.Email == adminEmail))
    {
        db.Users.Add(new User
        {
            Email = adminEmail,
            FullName = app.Configuration["Admin:FullName"] ?? "Administrateur",
            PasswordHash = Passwords.Hash(app.Configuration["Admin:Password"] ?? "Admin2026!"),
            Role = "Admin"
        });
        db.SaveChanges();
        app.Logger.LogInformation("Compte admin créé : {Email}", adminEmail);
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(uploads), RequestPath = "/photos" });
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/admin/"));

static int UserId(ClaimsPrincipal p) => int.Parse(p.FindFirstValue(ClaimTypes.NameIdentifier)!);
static bool IsEmail(string e) { try { return new MailAddress(e).Address == e; } catch { return false; } }
static IResult Bad(string msg) => Results.BadRequest(new { error = msg });

var api = app.MapGroup("/api");

// ---------- Test de connexion (utilisé par le bouton « Tester la connexion » de l'application) ----------
api.MapGet("/health", () => Results.Ok(new { ok = true, serverTimeUtc = DateTime.UtcNow }));

// ---------- Authentification ----------
api.MapPost("/auth/register", async (RegisterRequest r, AppDb db, TokenService tokens) =>
{
    var email = (r.Email ?? "").Trim().ToLowerInvariant();
    var name = (r.FullName ?? "").Trim();
    var pwd = r.Password ?? "";
    if (!IsEmail(email)) return Bad("Adresse email invalide.");
    if (name.Length < 2 || name.Length > 100) return Bad("Le nom complet est obligatoire.");
    if (pwd.Length < 6) return Bad("Le mot de passe doit contenir au moins 6 caractères.");
    if (await db.Users.AnyAsync(u => u.Email == email))
        return Results.Conflict(new { error = "Un compte existe déjà avec cet email." });

    var user = new User { Email = email, FullName = name, PasswordHash = Passwords.Hash(pwd) };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(new AuthResponse(tokens.Create(user), await Mapper.ToUserDto(db, user)));
});

api.MapPost("/auth/login", async (LoginRequest r, AppDb db, TokenService tokens) =>
{
    var email = (r.Email ?? "").Trim().ToLowerInvariant();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    if (user is null || !Passwords.Verify(r.Password ?? "", user.PasswordHash))
        return Results.Json(new { error = "Email ou mot de passe incorrect." }, statusCode: 401);
    return Results.Ok(new AuthResponse(tokens.Create(user), await Mapper.ToUserDto(db, user)));
});

api.MapPost("/auth/google", async (GoogleRequest r, AppDb db, TokenService tokens, IConfiguration cfg) =>
{
    // Le téléphone envoie le jeton d'identité (id_token) obtenu auprès de Google ; le serveur le vérifie lui-même.
    var audiences = (cfg.GetSection("Google:ClientIds").Get<string[]>() ?? Array.Empty<string>())
        .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
    if (audiences.Length == 0)
        return Results.Json(new { error = "Connexion Google non configurée sur le serveur (Google:ClientIds dans appsettings.json)." }, statusCode: 503);
    if (string.IsNullOrWhiteSpace(r.IdToken)) return Bad("Jeton Google manquant.");

    GoogleJsonWebSignature.Payload payload;
    try
    {
        payload = await GoogleJsonWebSignature.ValidateAsync(r.IdToken,
            new GoogleJsonWebSignature.ValidationSettings { Audience = audiences });
    }
    catch (InvalidJwtException)
    {
        return Results.Json(new { error = "Jeton Google invalide ou expiré." }, statusCode: 401);
    }
    if (!payload.EmailVerified || string.IsNullOrWhiteSpace(payload.Email)) return Bad("L'email du compte Google n'est pas vérifié.");

    var email = payload.Email.Trim().ToLowerInvariant();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    if (user is { Role: "Admin" }) return Results.Json(new { error = "Les comptes administrateur ne peuvent pas utiliser Google." }, statusCode: 403);
    if (user is null)
    {
        user = new User
        {
            Email = email,
            FullName = string.IsNullOrWhiteSpace(payload.Name) ? email.Split('@')[0] : payload.Name.Trim(),
            PasswordHash = Passwords.Hash(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)))   // mot de passe aléatoire inutilisable
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
    return Results.Ok(new AuthResponse(tokens.Create(user), await Mapper.ToUserDto(db, user)));
});

api.MapPost("/auth/forgot", (ForgotRequest r, ILogger<Program> log) =>
{
    // Pas d'envoi d'email en local : on journalise seulement. À brancher sur un service SMTP plus tard.
    log.LogInformation("Demande de réinitialisation pour {Email} (aucun email envoyé en local)", r.Email);
    return Results.Ok(new { ok = true });
});

// ---------- Utilisateur connecté ----------
var me = api.MapGroup("").RequireAuthorization();

me.MapGet("/me", async (ClaimsPrincipal p, AppDb db) =>
{
    var user = await db.Users.FindAsync(UserId(p));
    return user is null ? Results.Unauthorized() : Results.Ok(await Mapper.ToUserDto(db, user));
});

me.MapPost("/me/awards/{key}", async (string key, ClaimsPrincipal p, AppDb db) =>
{
    var rewards = new Dictionary<string, (int pts, string reason)> { ["guide"] = (50, "Guide de tri consulté") };
    if (!rewards.TryGetValue(key, out var reward)) return Results.NotFound(new { error = "Récompense inconnue." });
    var uid = UserId(p);
    if (await db.Awards.AnyAsync(a => a.UserId == uid && a.Key == key)) return Results.Ok(new { awarded = false });
    db.Awards.Add(new Award { UserId = uid, Key = key });
    db.Points.Add(new PointEntry { UserId = uid, Points = reward.pts, Reason = reward.reason });
    await db.SaveChangesAsync();
    return Results.Ok(new { awarded = true });
});

me.MapGet("/reports", async (ClaimsPrincipal p, AppDb db) =>
{
    var uid = UserId(p);
    var list = await db.Reports.Where(r => r.UserId == uid).OrderByDescending(r => r.CreatedAt).ToListAsync();
    return Results.Ok(list.Select(r => Mapper.ToDto(r)));
});

me.MapPost("/reports", async (HttpRequest req, ClaimsPrincipal p, AppDb db) =>
{
    if (!req.HasFormContentType) return Bad("Requête invalide.");
    var form = await req.ReadFormAsync();

    var description = form["description"].ToString().Trim();
    if (description.Length < 5 || description.Length > 1000) return Bad("La description doit contenir entre 5 et 1000 caractères.");

    var ci = CultureInfo.InvariantCulture;
    if (!double.TryParse(form["latitude"], NumberStyles.Float, ci, out var lat) ||
        !double.TryParse(form["longitude"], NumberStyles.Float, ci, out var lon) ||
        lat is < -90 or > 90 || lon is < -180 or > 180)
        return Bad("Position GPS invalide.");

    string? fileName = null;
    var photo = form.Files.GetFile("photo");
    if (photo is { Length: > 0 })
    {
        if (photo.Length > 10 * 1024 * 1024) return Bad("Photo trop volumineuse (10 Mo maximum).");
        var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp")) return Bad("Format de photo non supporté (jpg, png, webp).");
        fileName = $"{Guid.NewGuid():N}{ext}";           // nom généré par le serveur : pas de chemin fourni par l'utilisateur
        await using var fs = File.Create(Path.Combine(uploads, fileName));
        await photo.CopyToAsync(fs);
    }

    string Cut(string s, int max) => s.Length <= max ? s : s[..max];
    var quartier = Cut(form["quartier"].ToString().Trim(), 100);
    if (quartier.Length == 0) quartier = "Douala";

    var uid = UserId(p);
    var report = new Report
    {
        UserId = uid,
        Quartier = quartier,
        Address = Cut(form["address"].ToString().Trim(), 200),
        WasteType = Cut(form["wasteType"].ToString().Trim(), 60),
        Description = description,
        PhotoFileName = fileName,
        Latitude = lat, Longitude = lon,
        PointsEarned = 50
    };
    db.Reports.Add(report);
    db.Points.Add(new PointEntry { UserId = uid, Points = 50, Reason = $"Signalement envoyé à {quartier}" });
    await db.SaveChangesAsync();
    return Results.Ok(Mapper.ToDto(report));
});

// ---------- Administration ----------
var admin = api.MapGroup("/admin").RequireAuthorization(p => p.RequireRole("Admin"));

admin.MapGet("/server-info", (HttpContext ctx) =>
{
    // Adresses à saisir dans l'application : émulateur Android = 10.0.2.2, vrai téléphone = IP du PC sur le Wi-Fi
    var port = ctx.Request.Host.Port ?? 80;
    var ips = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
        .Where(n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up
                    && n.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
        .SelectMany(n => n.GetIPProperties().UnicastAddresses)
        .Where(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        .Select(a => $"http://{a.Address}:{port}")
        .Distinct().ToList();
    return Results.Ok(new { port, emulatorUrl = $"http://10.0.2.2:{port}", phoneUrls = ips });
});

admin.MapGet("/stats", async (AppDb db) => Results.Ok(new
{
    users = await db.Users.CountAsync(u => u.Role == "User"),
    reports = await db.Reports.CountAsync(),
    enCours = await db.Reports.CountAsync(r => r.Status == ReportStatus.EnCours),
    resolus = await db.Reports.CountAsync(r => r.Status == ReportStatus.Resolu)
}));

admin.MapGet("/reports", async (string? status, AppDb db) =>
{
    var q = db.Reports.Include(r => r.User).AsQueryable();
    if (Enum.TryParse<ReportStatus>(status, out var st)) q = q.Where(r => r.Status == st);
    var list = await q.OrderByDescending(r => r.CreatedAt).ToListAsync();
    return Results.Ok(list.Select(r => Mapper.ToDto(r, r.User)));
});

admin.MapGet("/users", async (AppDb db) =>
{
    var users = await db.Users.Include(u => u.Points).Include(u => u.Reports).OrderByDescending(u => u.CreatedAt).ToListAsync();
    return Results.Ok(users.Select(u => new
    {
        u.Id, u.Email, u.FullName, u.Role,
        createdAt = Mapper.Utc(u.CreatedAt),
        greenPoints = u.Points.Sum(x => x.Points),
        reports = u.Reports.Count
    }));
});

admin.MapPatch("/reports/{id:int}/status", async (int id, StatusRequest body, AppDb db) =>
{
    if (!Enum.TryParse<ReportStatus>(body.Status, out var status)) return Bad("Statut invalide (EnCours ou Resolu).");
    var report = await db.Reports.FindAsync(id);
    if (report is null) return Results.NotFound(new { error = "Signalement introuvable." });

    report.Status = status;
    if (status == ReportStatus.Resolu)
    {
        report.ResolvedAt ??= DateTime.UtcNow;
        if (!report.BonusAwarded)   // bonus +50 une seule fois quand le dépôt est nettoyé
        {
            report.BonusAwarded = true;
            report.PointsEarned += 50;
            db.Points.Add(new PointEntry { UserId = report.UserId, Points = 50, Reason = "Dépôt nettoyé par les services" });
        }
    }
    else report.ResolvedAt = null;

    await db.SaveChangesAsync();
    return Results.Ok(Mapper.ToDto(report));
});

app.Run();
