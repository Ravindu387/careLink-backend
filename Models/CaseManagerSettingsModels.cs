namespace CareLink.Backend.Models;

public class UserProfileSettingsDto
{
    public string FullName { get; set; } = "Amina Mensah";
    public string Email { get; set; } = "amina@response.org";
    public string Phone { get; set; } = "+94 77 412 8041";
    public string Area { get; set; } = "Greater Riverton";
    public string? ProfileImage { get; set; }
}

public class QueuePreferencesDto
{
    public string DefaultPriority { get; set; } = "All priorities";
    public string DefaultCategory { get; set; } = "All categories";
    public string DefaultStatus { get; set; } = "All statuses";
    public bool AutoRefresh { get; set; } = true;
    public bool ShowCriticalFirst { get; set; } = true;
    public bool ShowMyCasesOnly { get; set; } = false;
}

public class NotificationPreferencesSettingsDto
{
    public bool InAppNotifications { get; set; } = true;
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool CriticalAlerts { get; set; } = true;
    public bool AssignmentAlerts { get; set; } = true;
    public bool EvidenceUpdates { get; set; } = true;
    public bool EscalationAlerts { get; set; } = true;
    public bool AiRecommendations { get; set; } = false;
}

public class SecuritySettingsDto
{
    public bool TwoFactor { get; set; } = true;
    public bool LoginAlerts { get; set; } = true;
}

public class ActiveSessionDto
{
    public string Id { get; set; } = string.Empty;
    public string Device { get; set; } = string.Empty;
    public string Browser { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string LastActive { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}

public class CaseManagerSettingsDto
{
    public UserProfileSettingsDto Profile { get; set; } = new();
    public QueuePreferencesDto Queue { get; set; } = new();
    public NotificationPreferencesSettingsDto Notifications { get; set; } = new();
    public SecuritySettingsDto Security { get; set; } = new();
    public List<ActiveSessionDto> ActiveSessions { get; set; } = [];
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
