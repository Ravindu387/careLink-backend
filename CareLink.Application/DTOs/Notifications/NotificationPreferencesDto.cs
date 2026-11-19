namespace CareLink.Application.DTOs.Notifications;

public class NotificationPreferencesDto
{
    public bool EmailAlerts { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool EmergencyCriticalSound { get; set; } = true;
    public bool AssistanceRequestsNotify { get; set; } = true;
    public bool VolunteerCheckInsNotify { get; set; } = true;
    public bool LowResourceStockNotify { get; set; } = true;
    public bool DailySummaryDigest { get; set; } = true;
}
