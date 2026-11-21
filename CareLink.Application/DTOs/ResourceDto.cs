namespace CareLink.Application.DTOs;

public class ResourceDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Available { get; set; }
    public int Minimum { get; set; }
    public string Unit { get; set; } = "Units";
    public string Expiry { get; set; } = "—";
    public DateTime? ExpiryDate { get; set; }
    public string LastUpdated { get; set; } = "Today";
    public DateTime LastUpdatedUtc { get; set; }
    public string Shelter { get; set; } = string.Empty;
    public string ShelterId { get; set; } = string.Empty;
    public string Status { get; set; } = "Healthy";
}
