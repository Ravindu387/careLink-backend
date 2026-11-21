namespace CareLink.Core.Entities;

public class EmergencyNotification
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = "Incident"; // Incident, Assistance, AI Alert, Weather, System
    public string Severity { get; set; } = "Warning"; // Critical, Warning, Info
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public bool IsRead { get; set; } = false;
    public string? ActionUrl { get; set; }
    public string? ActionLabel { get; set; }
}

public class EmergencyHelpline
{
    public string Name { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool Is24x7 { get; set; } = true;
}

public class GlobalSearchResult
{
    public string Category { get; set; } = string.Empty; // Crisis Incident, Assistance Request, Location, Coordinator
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class EmergencyTopbarState
{
    public string OperationalArea { get; set; } = "Greater Riverton";
    public string SelectedLanguage { get; set; } = "EN";
    public List<string> SupportedLanguages { get; set; } = new() { "EN", "SI", "TA" };
    public int UnreadNotificationCount { get; set; } = 14;
    public List<EmergencyNotification> Notifications { get; set; } = new();
    public List<EmergencyHelpline> Helplines { get; set; } = new();
    public string CoordinatorName { get; set; } = "Amina Mensah";
    public string CoordinatorRole { get; set; } = "Emergency Coordinator";
    public string CoordinatorInitials { get; set; } = "AM";
    public string DutyStatus { get; set; } = "On Duty";
}
