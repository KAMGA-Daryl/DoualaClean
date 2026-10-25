using System.Text.Json;

namespace DoualaClean.Services;

/// <summary>Mode AUTONOME : signalements enregistrés dans un fichier JSON sur le téléphone.</summary>
public sealed class LocalReportService : IReportService
{
    private const int PointsPerReport = 50;
    private readonly LocalAuthService _auth;
    private readonly string _file = Path.Combine(FileSystem.AppDataDirectory, "local_reports.json");
    private readonly List<WasteReport> _reports;

    public LocalReportService(LocalAuthService auth)
    {
        _auth = auth;
        _reports = Load();
        if (_reports.Count == 0) SeedDemoReports();
    }

    public Task<IReadOnlyList<WasteReport>> GetMineAsync()
    {
        var email = _auth.CurrentUser?.Email;
        IReadOnlyList<WasteReport> list = _reports
            .Where(r => r.OwnerEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.CreatedAt).ToList();
        return Task.FromResult(list);
    }

    public WasteReport? GetById(int id) => _reports.FirstOrDefault(r => r.Id == id);

    public async Task<WasteReport> SubmitAsync(ReportDraft draft)
    {
        if (_auth.CurrentUser is null) throw new InvalidOperationException("Utilisateur non connecté.");
        await Task.Delay(500); // simule l'envoi

        var report = new WasteReport
        {
            Id = (_reports.Count == 0 ? 0 : _reports.Max(r => r.Id)) + 1,
            OwnerEmail = _auth.CurrentUser.Email,
            Quartier = string.IsNullOrWhiteSpace(draft.Quartier) ? "Douala" : draft.Quartier,
            Address = draft.Address,
            WasteType = draft.WasteType,
            Description = draft.Description,
            PhotoUrl = draft.PhotoPath,
            Latitude = draft.Latitude,
            Longitude = draft.Longitude,
            Status = ReportStatus.EnCours,
            CreatedAt = DateTime.Now,
            PointsEarned = PointsPerReport
        };
        _reports.Add(report);
        Save();
        _auth.AddPoints(PointsPerReport, $"Signalement envoyé à {report.Quartier}");
        return report;
    }

    private List<WasteReport> Load()
    {
        try
        {
            if (File.Exists(_file))
                return JsonSerializer.Deserialize<List<WasteReport>>(File.ReadAllText(_file)) ?? new();
        }
        catch { /* fichier illisible : on repart de zéro */ }
        return new();
    }

    private void Save() => File.WriteAllText(_file, JsonSerializer.Serialize(_reports));

    // Données de démonstration reprenant les maquettes (#01 en cours, #02 résolu)
    private void SeedDemoReports()
    {
        _reports.Add(new WasteReport
        {
            Id = 1, OwnerEmail = LocalAuthService.DemoEmail, Quartier = "Akwa", Address = "Boulevard de la Liberté, Akwa",
            WasteType = "Plastiques & emballages",
            Description = "Dépôt sauvage de déchets plastiques et ordures ménagères bloquant le passage.",
            PhotoUrl = "sample_report.jpg",
            Latitude = 4.0511, Longitude = 9.7679, Status = ReportStatus.EnCours,
            CreatedAt = DateTime.Now.AddDays(-2)
        });
        _reports.Add(new WasteReport
        {
            Id = 2, OwnerEmail = LocalAuthService.DemoEmail, Quartier = "Bonapriso", Address = "Rue Joss, Bonapriso",
            WasteType = "Verre & métaux",
            Description = "Bouteilles et canettes entassées au bord de la route.",
            Latitude = 4.0285, Longitude = 9.6950, Status = ReportStatus.Resolu,
            CreatedAt = DateTime.Now.AddDays(-9), PointsEarned = 50
        });
        Save();
    }
}
