namespace CareLink.Backend.Models;

public class VolunteerAnalyticsSummaryDto
{
    public int TasksCompleted { get; set; } = 1284;
    public int VolunteerHours { get; set; } = 14280;
    public double AcceptanceRate { get; set; } = 92.0;
    public double IncidentFreeRate { get; set; } = 98.7;
    public int ActiveVolunteers { get; set; } = 2184;
    public int TotalShifts { get; set; } = 3418;
    public string CompletionRate { get; set; } = "89.4% completion";
    public string HoursComparison { get; set; } = "+12% vs previous week";
    public string InvitationsCount { get; set; } = "3,418 invitations";
    public string IncidentsSummary { get; set; } = "18 minor incidents";
}

public class TrendDataPoint
{
    public string Date { get; set; } = string.Empty;
    public int Value { get; set; }
    public int Created { get; set; }
    public int Completed { get; set; }
}

public class SkillHoursBreakdown
{
    public string Name { get; set; } = string.Empty;
    public int Hours { get; set; }
    public double Percentage { get; set; }
}

public class VolunteerReportItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string IconName { get; set; } = "FileText";
    public string LastGenerated { get; set; } = "Today";
    public string Format { get; set; } = "CSV";
}

public class VolunteerReportsOverviewDto
{
    public VolunteerAnalyticsSummaryDto Analytics { get; set; } = new();
    public List<TrendDataPoint> TrendData { get; set; } = [];
    public List<SkillHoursBreakdown> SkillData { get; set; } = [];
    public List<VolunteerReportItem> Reports { get; set; } = [];
    public List<string> OperationalAreas { get; set; } = [];
    public List<string> SkillGroups { get; set; } = [];
    public List<string> DateRanges { get; set; } = [];
}

public class VolunteerReportsQueryDto
{
    public string? DateRange { get; set; } = "18-25 June 2025";
    public string? OperationalArea { get; set; } = "All areas";
    public string? SkillGroup { get; set; } = "All skills";
}
