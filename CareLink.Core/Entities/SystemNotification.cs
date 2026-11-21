namespace CareLink.Core.Entities;

public class SystemNotification
{
    public int Id { get; set; }
    public string Type { get; set; } = "System"; // Emergency, Assistance, Volunteer, Resource, System
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal"; // Critical, High, Normal, Low
    public string? Location { get; set; }
    public string? Action { get; set; }
    public string? ActionUrl { get; set; }
    public bool Unread { get; set; } = true;
    public bool IsArchived { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public string? TargetUserRole { get; set; }
    public string? CreatedBy { get; set; }
}
