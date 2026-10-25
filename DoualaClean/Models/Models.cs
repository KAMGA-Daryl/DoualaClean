using System.Text.Json.Serialization;

namespace DoualaClean.Models;

public enum ReportStatus { EnCours, Resolu }

public class PointsEntry
{
    public int Points { get; set; }
    public string Reason { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Now;
    [JsonIgnore] public string Display => $"+{Points} pts : {Reason}";
}

public class AppUser
{
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public List<PointsEntry> PointsHistory { get; set; } = new();
    public List<string> Awards { get; set; } = new();

    [JsonIgnore] public int GreenPoints => PointsHistory.Sum(p => p.Points);

    [JsonIgnore]
    public string Level => GreenPoints switch
    {
        < 100 => "Éco-débutant",
        < 300 => "Éco-citoyen actif",
        _ => "Éco-champion"
    };

    [JsonIgnore]
    public string Initials => string.Concat(FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                                    .Take(2).Select(p => char.ToUpperInvariant(p[0])));
}

public class WasteReport
{
    public int Id { get; set; }
    public string OwnerEmail { get; set; } = "";
    public string Quartier { get; set; } = "";
    public string Address { get; set; } = "";
    public string WasteType { get; set; } = "";
    public string Description { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int PointsEarned { get; set; }

    [JsonIgnore] public bool IsResolved => Status == ReportStatus.Resolu;
    [JsonIgnore] public string StatusIcon => IsResolved ? "🟢" : "🟡";
    [JsonIgnore] public string StatusLabel => IsResolved ? "Résolu (collecté par l'équipe)" : "En cours de traitement";
    [JsonIgnore] public string Title => $"Signalement {Id:00} - {Quartier}";
    [JsonIgnore] public bool HasPhoto => !string.IsNullOrEmpty(PhotoUrl);

    [JsonIgnore]
    public string RelativeDate
    {
        get
        {
            var d = DateTime.UtcNow - CreatedAt.ToUniversalTime();
            if (d.TotalMinutes < 1) return "À l'instant";
            if (d.TotalHours < 1) return $"Il y a {(int)d.TotalMinutes} min";
            if (d.TotalDays < 1) return $"Il y a {(int)d.TotalHours} h";
            var days = (int)d.TotalDays;
            return days == 1 ? "Il y a 1 jour" : $"Il y a {days} jours";
        }
    }
}

public class ReportDraft
{
    public string Description { get; set; } = "";
    public string WasteType { get; set; } = "";
    public string? PhotoPath { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = "";
    public string Quartier { get; set; } = "";
}

public class CollectionBin
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public string WasteType { get; set; } = "";
    public int FillPercent { get; set; }
    public string NextCollection { get; set; } = "";
    public string Responsible { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Image { get; set; } = "";

    public double FillRatio => FillPercent / 100.0;
    public bool IsAlert => FillPercent >= 80;
    public string StatusText => IsAlert ? $"⚠️ Rempli à {FillPercent}%" : $"✅ Rempli à {FillPercent}%";
}

public class SortingCategory
{
    public string Key { get; set; } = "";
    public string Title { get; set; } = "";
    public string Icon { get; set; } = "";
    public string[] WhatToThrow { get; set; } = Array.Empty<string>();
    public string[] Rules { get; set; } = Array.Empty<string>();
}

public record AuthResult(bool Success, string Error = "");
