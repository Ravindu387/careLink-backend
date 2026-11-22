namespace CareLink.Application.DTOs;

public class ResourceDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Available { get; set; }
    public int Reserved { get; set; }
    public int Availability { get; set; }
    public int Minimum { get; set; }
    public string Unit { get; set; } = "Units";
    public string Expiry { get; set; } = "—";
    public DateTime? ExpiryDate { get; set; }
    public string LastUpdated { get; set; } = "Today";
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    public DateTimeOffset LastRestocked { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Shelter { get; set; } = string.Empty;
    public string ShelterId { get; set; } = string.Empty;
    public string Status { get; set; } = "Healthy";
}
