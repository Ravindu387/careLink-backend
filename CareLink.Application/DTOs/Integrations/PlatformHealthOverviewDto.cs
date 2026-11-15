namespace CareLink.Application.DTOs.Integrations;

public class PlatformHealthOverviewDto
{
    public string CorePlatformStatus { get; set; } = "Operational"; // Operational, Degraded, Unavailable
    public int ServicesHealthyPercentage { get; set; } = 100;
    public string ApiAvailability { get; set; } = "99.94%";
    public string MedianLatency { get; set; } = "248 ms";
    public int TotalServices { get; set; }
    public int OperationalCount { get; set; }
    public int DegradedCount { get; set; }
    public int UnavailableCount { get; set; }
    public string LastChecked { get; set; } = "Just now";
    public DateTime LastCheckedAt { get; set; } = DateTime.UtcNow;
}
