namespace CareLink.Core.Entities;

public class SecurityEvent
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Severity { get; set; } = "Low"; // High, Medium, Low
    public string Status { get; set; } = "Investigating"; // Investigating, Review Required, Resolved
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
    public string? ResolutionNote { get; set; }
    public string? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string Environment { get; set; } = "Production";
}
