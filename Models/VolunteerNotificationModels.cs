namespace CareLink.Backend.Models;

public class VolunteerNotification
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Category { get; set; } = "Assignments"; // "Assignments", "Verification", "Safety", "AI Matching", "Communications"
    public string Color { get; set; } = "bg-[#e5f5ef] text-[#239879]";
    public string IconName { get; set; } = "Check";
    public bool Unread { get; set; } = true;
    public bool Urgent { get; set; } = false;
    public string Details { get; set; } = string.Empty;
    public string? TargetId { get; set; }
}

public class VolunteerNotificationSummaryDto
{
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int UrgentCount { get; set; }
    public int AssignmentCount { get; set; }
    public int VerificationCount { get; set; }
    public int SafetyCount { get; set; }
    public int AiMatchingCount { get; set; }
    public int CommunicationsCount { get; set; }
    public List<DeliveryServiceStatusDto> DeliveryServices { get; set; } = [];
}

public class VolunteerNotificationPreferencesDto
{
    public bool Assignments { get; set; } = true;
    public bool Verification { get; set; } = true;
    public bool Safety { get; set; } = true;
    public bool AiMatching { get; set; } = true;
    public bool Communications { get; set; } = true;
    public bool InApp { get; set; } = true;
    public bool Email { get; set; } = true;
    public bool Sms { get; set; } = false;
    public bool UrgentOnly { get; set; } = false;
}

public class VolunteerNotificationQueryDto
{
    public string? Tab { get; set; } = "All";
    public string? Search { get; set; }
    public string? Category { get; set; }
    public bool? UnreadOnly { get; set; }
    public bool? UrgentOnly { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 6;
}

public class CreateVolunteerNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "Assignments";
    public string Details { get; set; } = string.Empty;
    public bool Urgent { get; set; } = false;
    public string? TargetId { get; set; }
    public string? IconName { get; set; }
    public string? Color { get; set; }
}
