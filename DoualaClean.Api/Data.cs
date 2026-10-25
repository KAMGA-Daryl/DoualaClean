using Microsoft.EntityFrameworkCore;

namespace DoualaClean.Api;

public enum ReportStatus { EnCours, Resolu }

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "User";   // "User" | "Admin"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<PointEntry> Points { get; set; } = new();
    public List<Report> Reports { get; set; } = new();
}

public class PointEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Points { get; set; }
    public string Reason { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow;
}

public class Award
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Key { get; set; } = "";
}

public class Report
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string Quartier { get; set; } = "";
    public string Address { get; set; } = "";
    public string WasteType { get; set; } = "";
    public string Description { get; set; } = "";
    public string? PhotoFileName { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.EnCours;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public int PointsEarned { get; set; }
    public bool BonusAwarded { get; set; }
}

public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<PointEntry> Points => Set<PointEntry>();
    public DbSet<Award> Awards => Set<Award>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<Award>().HasIndex(a => new { a.UserId, a.Key }).IsUnique();
        b.Entity<Report>().Property(r => r.Status).HasConversion<string>();
    }
}
