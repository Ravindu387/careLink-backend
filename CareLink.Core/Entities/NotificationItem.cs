namespace CareLink.Core.Entities;

public class NotificationItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Category { get; set; } = "critical"; // critical, ai, resource, shelter, partner, report, urgent, collaboration, reporting, agreement, team
    public bool Unread { get; set; } = true;
    public bool Urgent { get; set; } = false;
    public string Reference { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? ActionLabel { get; set; }
    public string? TargetRole { get; set; } = "all"; // "all", "coordinator", "partner"
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReadAt { get; set; }
}
