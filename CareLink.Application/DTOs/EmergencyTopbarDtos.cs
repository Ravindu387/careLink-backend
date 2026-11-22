namespace CareLink.Application.DTOs;

public class EmergencyNotificationDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionLabel { get; set; }
}

public class EmergencyHelplineDto
{
    public string Name { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool Is24x7 { get; set; }
}

public class GlobalSearchResultDto
{
    public string Category { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class EmergencyTopbarStateDto
{
    public string OperationalArea { get; set; } = string.Empty;
    public string SelectedLanguage { get; set; } = string.Empty;
    public List<string> SupportedLanguages { get; set; } = new();
    public int UnreadNotificationCount { get; set; }
    public List<EmergencyNotificationDto> Notifications { get; set; } = new();
    public List<EmergencyHelplineDto> Helplines { get; set; } = new();
    public string CoordinatorName { get; set; } = string.Empty;
    public string CoordinatorRole { get; set; } = string.Empty;
    public string CoordinatorInitials { get; set; } = string.Empty;
    public string DutyStatus { get; set; } = string.Empty;
}

public class UpdateAreaRequest
{
    public string OperationalArea { get; set; } = string.Empty;
}

public class UpdateLanguageRequest
{
    public string Language { get; set; } = "EN";
}
