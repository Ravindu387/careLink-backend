using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class PartnerReportDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string Submitted { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string? Description { get; set; }
    public string? OrganizationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CreatePartnerReportDto
{
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Type { get; set; } = "Daily Report";

    [Required]
    [StringLength(100)]
    public string Owner { get; set; } = "Sara Khan";

    [Required]
    [StringLength(100)]
    public string Period { get; set; } = string.Empty;

    public string? Status { get; set; } = "Draft";
    public int? Progress { get; set; } = 0;
    public string? Description { get; set; }
}

public class UpdatePartnerReportDto
{
    [StringLength(150)]
    public string? Title { get; set; }

    [StringLength(100)]
    public string? Type { get; set; }

    [StringLength(100)]
    public string? Owner { get; set; }

    [StringLength(100)]
    public string? Period { get; set; }

    public string? Status { get; set; }
    public int? Progress { get; set; }
    public string? Description { get; set; }
}

public class UpdatePartnerReportStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public class WeeklyReachDto
{
    public string Day { get; set; } = string.Empty;
    public int Planned { get; set; }
    public int Reached { get; set; }
}

public class PerformanceTargetDto
{
    public string Title { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class ReportingScheduleDto
{
    public string Name { get; set; } = string.Empty;
    public string Deadline { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class DailyReportReminderDto
{
    public string Title { get; set; } = string.Empty;
    public string Deadline { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public int Progress { get; set; }
}

public class PartnerReportsAnalyticsDto
{
    public string ReportsSubmitted { get; set; } = "28";
    public string OnTimeRate { get; set; } = "96%";
    public string OnTimeTrend { get; set; } = "↑ 4% this month";
    public string PeopleReached { get; set; } = "18.6K";
    public string PeopleReachedSubtitle { get; set; } = "Across 4 sectors";
    public string NextDeadline { get; set; } = "4h 12m";
    public string NextDeadlineSubtitle { get; set; } = "Daily activity report";

    public DailyReportReminderDto Reminder { get; set; } = new();
    public List<WeeklyReachDto> WeeklyReach { get; set; } = new();
    public List<PerformanceTargetDto> PerformanceTargets { get; set; } = new();
    public List<ReportingScheduleDto> Schedules { get; set; } = new();

    public int TotalReports { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }

    public DateTimeOffset LastUpdated { get; set; }
}

public class PartnerReportQueryParameters
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
}
