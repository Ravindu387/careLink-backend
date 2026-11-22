namespace CareLink.Core.Entities;

public class PartnerReport
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = "Daily Report"; // Daily Report, Operational Report, Sector Report, Assessment Report
    public string Owner { get; set; } = "Sara Khan";
    public string Period { get; set; } = string.Empty;
    public string Submitted { get; set; } = "Not submitted";
    public string Status { get; set; } = "Draft"; // Draft, Under Review, Submitted, Approved, Archived
    public int Progress { get; set; } = 0;
    public string? Description { get; set; }
    public string? OrganizationId { get; set; } = "ORG-027";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class WeeklyReachDataPoint
{
    public string Day { get; set; } = string.Empty;
    public int Planned { get; set; }
    public int Reached { get; set; }
}

public class PerformanceTargetItem
{
    public string Title { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class ReportingScheduleItem
{
    public string Name { get; set; } = string.Empty;
    public string Deadline { get; set; } = string.Empty;
    public string Status { get; set; } = "Upcoming"; // Due today, Upcoming, Completed
}

public class DailyReportReminder
{
    public string Title { get; set; } = "Daily Partner Activity Report · Monsoon Floods 2025";
    public string Deadline { get; set; } = "DUE TODAY - 18:00";
    public string Subtitle { get; set; } = "Progress saved · 82% complete · 3 sections require review";
    public int Progress { get; set; } = 82;
}

public class PartnerReportsAnalytics
{
    public string ReportsSubmitted { get; set; } = "28";
    public string OnTimeRate { get; set; } = "96%";
    public string OnTimeTrend { get; set; } = "↑ 4% this month";
    public string PeopleReached { get; set; } = "18.6K";
    public string PeopleReachedSubtitle { get; set; } = "Across 4 sectors";
    public string NextDeadline { get; set; } = "4h 12m";
    public string NextDeadlineSubtitle { get; set; } = "Daily activity report";

    public DailyReportReminder Reminder { get; set; } = new();
    public List<WeeklyReachDataPoint> WeeklyReach { get; set; } = new();
    public List<PerformanceTargetItem> PerformanceTargets { get; set; } = new();
    public List<ReportingScheduleItem> Schedules { get; set; } = new();

    public int TotalReports { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }

    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
