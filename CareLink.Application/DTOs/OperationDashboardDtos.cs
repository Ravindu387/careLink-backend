using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class DashboardKpiDto
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Icon { get; set; } = "Inbox";
    public string Color { get; set; } = "blue";
    public string Category { get; set; } = string.Empty;
    public double? NumericValue { get; set; }
}

public class ActiveCrisisBriefingDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public int Level { get; set; }
    public string ActiveSince { get; set; } = string.Empty;
    public int AffectedDistrictsCount { get; set; }
    public string LeadAgency { get; set; } = string.Empty;
    public string NextBriefing { get; set; } = string.Empty;
    public string NextBriefingTime { get; set; } = string.Empty;
}

public class WeeklyRequestTrendDto
{
    public string Day { get; set; } = string.Empty;
    public int Received { get; set; }
    public int Fulfilled { get; set; }
    public DateTimeOffset? Date { get; set; }
}

public class RecentActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Status { get; set; } = "New";
    public DateTimeOffset Timestamp { get; set; }
    public string? ReferenceId { get; set; }
}

public class UpcomingBriefingDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public DateTimeOffset? ScheduledAt { get; set; }
    public string LeadOrganisation { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int AttendeesCount { get; set; }
}

public class OperationDashboardDto
{
    public ActiveCrisisBriefingDto Crisis { get; set; } = new();
    public List<DashboardKpiDto> Kpis { get; set; } = new();
    public List<WeeklyRequestTrendDto> WeeklyTrends { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    public List<UpcomingBriefingDto> UpcomingBriefings { get; set; } = new();
    public int ActiveOperationalPointsCount { get; set; }
    public DateTimeOffset LastSyncedAt { get; set; }
}

public class CreateActivityRequestDto
{
    [Required]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Location { get; set; } = string.Empty;

    public string? District { get; set; }

    [RegularExpression("^(New|In Progress|Completed|Pending|Escalated)$", ErrorMessage = "Status must be New, In Progress, Completed, Pending, or Escalated.")]
    public string Status { get; set; } = "New";

    public string? ReferenceId { get; set; }
}

public class LogIncidentRequestDto
{
    [Required]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string District { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string Location { get; set; } = string.Empty;

    [RegularExpression("^(Critical|Major|Moderate|Minor)$", ErrorMessage = "Severity must be Critical, Major, Moderate, or Minor.")]
    public string Severity { get; set; } = "Major";
}
