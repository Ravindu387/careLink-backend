namespace CareLink.Application.DTOs.Notifications;

public class NotificationStatsDto
{
    public int Total { get; set; }
    public int UnreadCount { get; set; }
    public int CriticalCount { get; set; }
    public int HighPriorityCount { get; set; }
    public int EmergencyCount { get; set; }
    public int AssistanceCount { get; set; }
    public int VolunteerCount { get; set; }
    public int ResourceCount { get; set; }
    public int SystemCount { get; set; }
}
