namespace CareLink.Application.DTOs.Notifications;

public class NotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = "System"; // Emergency, Assistance, Volunteer, Resource, System
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal"; // Critical, High, Normal
    public string Time { get; set; } = string.Empty; // "2 min ago", "14 min ago", etc.
    public string Date { get; set; } = "Today"; // "Today", "Yesterday", or formatted date
    public DateTime CreatedAt { get; set; }
    public string? Location { get; set; }
    public bool Unread { get; set; }
    public string? Action { get; set; }
    public string? ActionUrl { get; set; }
}
