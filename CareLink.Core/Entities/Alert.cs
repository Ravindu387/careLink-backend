namespace CareLink.Core.Entities;

public class Alert
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Time { get; set; } = "Just now";
    public string Severity { get; set; } = "Warning";
    public string Status { get; set; } = "Open";
    public string Icon { get; set; } = "alert";
    public string? AssignedTo { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
