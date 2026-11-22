namespace CareLink.Core.Entities;

public class CitizenNotification
{
    public string Id { get; set; } = string.Empty;
    public string CitizenName { get; set; } = "Samira Khan";
    public string CitizenId { get; set; } = "CIT-001";
    public string Type { get; set; } = "alert"; // "alert", "message", "delivery", "shelter", "medical"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "Safety alerts"; // "Safety alerts", "My requests"
    public bool Unread { get; set; } = true;
    public string? ActionUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}
