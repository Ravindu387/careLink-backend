namespace CareLink.Core.Entities;

public class SafetyUpdate
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "Emergency alert";
    public string Level { get; set; } = "Emergency alert"; // Emergency alert, Warning, Service update, Advisory
    public string Severity { get; set; } = "Critical"; // Critical, High, Medium, Low
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Area { get; set; } = "Greater Riverton";
    public string Source { get; set; } = "Disaster Management Centre (DMC)";
    public string? ActionInstructions { get; set; }
    public List<string> SafeRoutes { get; set; } = new();
    public List<string> HazardAreas { get; set; } = new();
    public string Helpline { get; set; } = "112";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
