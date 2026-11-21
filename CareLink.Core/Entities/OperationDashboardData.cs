namespace CareLink.Core.Entities;

public class DashboardKpi
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Icon { get; set; } = "Inbox";
    public string Color { get; set; } = "blue"; // red, blue, amber, green, purple
    public string Category { get; set; } = string.Empty;
    public double? NumericValue { get; set; }
}

public class ActiveCrisisBriefing
{
    public string Id { get; set; } = "CR-2025-001";
    public string Title { get; set; } = "Monsoon Floods 2025";
    public string Severity { get; set; } = "SEVERITY 4 - MAJOR";
    public int Level { get; set; } = 4;
    public string ActiveSince { get; set; } = "Active since 18 Jun · 4 districts affected";
    public int AffectedDistrictsCount { get; set; } = 4;
    public string LeadAgency { get; set; } = "National Disaster Authority";
    public string NextBriefing { get; set; } = "14:30 · in 42 min";
    public string NextBriefingTime { get; set; } = "14:30";
}

public class WeeklyRequestTrend
{
    public string Day { get; set; } = string.Empty;
    public int Received { get; set; }
    public int Fulfilled { get; set; }
    public DateTimeOffset? Date { get; set; }
}

public class RecentOperationActivity
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Status { get; set; } = "New"; // New, In Progress, Completed, Pending, Escalated
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string? ReferenceId { get; set; }
}

public class UpcomingBriefing
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public DateTimeOffset? ScheduledAt { get; set; }
    public string LeadOrganisation { get; set; } = "National Disaster Authority";
    public string Location { get; set; } = "Command HQ & Virtual Briefing Room";
    public int AttendeesCount { get; set; } = 18;
}

public class OperationDashboardState
{
    public ActiveCrisisBriefing Crisis { get; set; } = new();
    public List<DashboardKpi> Kpis { get; set; } = new();
    public List<WeeklyRequestTrend> WeeklyTrends { get; set; } = new();
    public List<RecentOperationActivity> RecentActivities { get; set; } = new();
    public List<UpcomingBriefing> UpcomingBriefings { get; set; } = new();
    public int ActiveOperationalPointsCount { get; set; } = 342;
    public DateTimeOffset LastSyncedAt { get; set; } = DateTimeOffset.UtcNow;
}
