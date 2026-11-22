namespace CareLink.Backend.Models;

public class NotificationItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Type { get; set; } = "Assignment"; // "Critical", "Assignment", "Duplicate", "Evidence", "Escalation", "AI"
    public bool Read { get; set; }
    public string CaseId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DeliveryServiceStatusDto
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Operational"; // "Operational", "Degraded", "Offline"
    public string Message { get; set; } = string.Empty;
}

public class NotificationSummaryDto
{
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int CriticalCount { get; set; }
    public int AssignmentCount { get; set; }
    public int EvidenceCount { get; set; }
    public int DuplicateCount { get; set; }
    public int EscalationCount { get; set; }
    public int AiCount { get; set; }
    public List<DeliveryServiceStatusDto> DeliveryServices { get; set; } = [];
}

public class NotificationPreferencesDto
{
    public bool InAppEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = true;
    public bool PushEnabled { get; set; } = true;
    public bool CriticalEnabled { get; set; } = true;
    public bool AssignmentEnabled { get; set; } = true;
    public bool EvidenceEnabled { get; set; } = true;
}

public class NotificationQueryDto
{
    public string? Tab { get; set; } = "All";
    public string? Search { get; set; }
    public string? Type { get; set; }
    public bool? UnreadOnly { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 6;
}

public class CreateNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "Assignment";
    public string CaseId { get; set; } = string.Empty;
}
