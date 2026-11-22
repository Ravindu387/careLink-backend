namespace CareLink.Application.DTOs;

public class CoordinatorProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DutyStatus { get; set; } = string.Empty;
    public string OperationalArea { get; set; } = string.Empty;
    public string BadgeNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime ShiftStartedAt { get; set; }
    public string ShiftDuration { get; set; } = string.Empty;
}

public class ActiveCrisisSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortTitle { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public int Level { get; set; }
    public string OperationalArea { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int AffectedPeople { get; set; }
    public string FormattedAffectedPeople { get; set; } = string.Empty;
    public string StartedDate { get; set; } = string.Empty;
}

public class SidebarBadgeCountsDto
{
    public int CrisisEvents { get; set; }
    public int AssistanceRequests { get; set; }
    public int PriorityQueue { get; set; }
    public int AiRecommendations { get; set; }
    public int Notifications { get; set; }
    public int AuditTimeline { get; set; }
}

public class SystemReadinessDto
{
    public string Status { get; set; } = string.Empty;
    public bool IsActiveResponse { get; set; }
    public DateTime LastSyncedAt { get; set; }
    public int ActiveOperationsCount { get; set; }
    public string Heartbeat { get; set; } = string.Empty;
}

public class EmergencySidebarStateDto
{
    public CoordinatorProfileDto Coordinator { get; set; } = new();
    public ActiveCrisisSummaryDto ActiveCrisis { get; set; } = new();
    public SidebarBadgeCountsDto Badges { get; set; } = new();
    public SystemReadinessDto System { get; set; } = new();
}

public class UpdateDutyStatusRequest
{
    public string DutyStatus { get; set; } = "On Duty";
}

public class SetActiveCrisisRequest
{
    public string CrisisId { get; set; } = string.Empty;
}
