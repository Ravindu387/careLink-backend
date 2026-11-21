namespace CareLink.Application.DTOs.Integrations;

public class IntegrationHealthDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Availability { get; set; } = "99.9%";
    public string Latency { get; set; } = "—";
    public double LatencyMs { get; set; }
    public string Health { get; set; } = "Operational"; // Operational, Degraded, Unavailable
    public string LastCheck { get; set; } = "Just now";
    public DateTime LastCheckedAt { get; set; } = DateTime.UtcNow;
    public string Icon { get; set; } = "KeyRound";
    public string? Endpoint { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
}
