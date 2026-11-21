namespace CareLink.Application.DTOs;

public class CitizenNotificationDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "alert";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public bool Unread { get; set; } = true;
    public string Category { get; set; } = "Safety alerts";
    public string? ActionUrl { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class CreateNotificationRequest
{
    public string Type { get; set; } = "alert";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "Safety alerts";
    public string? ActionUrl { get; set; }
    public string? CitizenName { get; set; }
}

public class NotificationSummaryDto
{
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int SafetyAlertsCount { get; set; }
    public int MyRequestsCount { get; set; }
}
