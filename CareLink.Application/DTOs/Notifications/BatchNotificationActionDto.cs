namespace CareLink.Application.DTOs.Notifications;

public class BatchNotificationActionDto
{
    public List<int> NotificationIds { get; set; } = new();
    public string Action { get; set; } = "mark_read"; // "mark_read", "mark_unread", "archive", "delete"
}
