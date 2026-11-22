using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class NotificationDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Category { get; set; } = "critical";
    public bool Unread { get; set; } = true;
    public bool Urgent { get; set; } = false;
    public string Reference { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? ActionLabel { get; set; }
    public string? TargetRole { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class NotificationPreferencesDto
{
    // Coordinator settings
    public bool Critical { get; set; } = true;
    public bool Ai { get; set; } = true;
    public bool Resources { get; set; } = true;
    public bool Shelter { get; set; } = true;
    public bool Partner { get; set; } = true;
    public bool Reports { get; set; } = true;

    // Partner Officer settings
    public bool Urgent { get; set; } = true;
    public bool Referrals { get; set; } = true;
    public bool Collaboration { get; set; } = true;
    public bool Reporting { get; set; } = true;
    public bool Team { get; set; } = true;
    public bool Email { get; set; } = false;
    public bool Push { get; set; } = true;

    public DateTimeOffset? UpdatedAt { get; set; }
}

public class NotificationMetricsDto
{
    public int All { get; set; }
    public int Unread { get; set; }
    public int Urgent { get; set; }
    public int Critical { get; set; }
    public int Ai { get; set; }
    public int Resource { get; set; }
    public int Shelter { get; set; }
    public int Partner { get; set; }
    public int Report { get; set; }
    public int Collaboration { get; set; }
    public int Reporting { get; set; }
    public int Team { get; set; }
    public int Agreement { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}

public class PagedNotificationsDto
{
    public IReadOnlyList<NotificationDto> Items { get; set; } = new List<NotificationDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int UnreadCount { get; set; }
}

public class UpdateNotificationPreferencesDto
{
    public bool? Critical { get; set; }
    public bool? Ai { get; set; }
    public bool? Resources { get; set; }
    public bool? Shelter { get; set; }
    public bool? Partner { get; set; }
    public bool? Reports { get; set; }

    public bool? Urgent { get; set; }
    public bool? Referrals { get; set; }
    public bool? Collaboration { get; set; }
    public bool? Reporting { get; set; }
    public bool? Team { get; set; }
    public bool? Email { get; set; }
    public bool? Push { get; set; }
}

// ── Citizen Notifications (Oshadi) ──────────────────────
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
