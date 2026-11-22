namespace CareLink.Backend.Models;

public class CaseItem
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Applicant { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string AiPriority { get; set; } = "Medium";
    public string Confidence { get; set; } = "85% confidence";
    public string Verification { get; set; } = "Evidence pending";
    public string Assigned { get; set; } = "Unassigned";
    public string Age { get; set; } = "Just now";
    public string Status { get; set; } = "Submitted";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class StatCardItem
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;
    public string Color { get; set; } = "blue";
}

public class StatusBreakdownItem
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class ChartDataPoint
{
    public string Day { get; set; } = string.Empty;
    public int Received { get; set; }
    public int Resolved { get; set; }
}

public class ActivityItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Time { get; set; } = "Just now";
    public string IconKey { get; set; } = "Activity";
    public string Color { get; set; } = "blue";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class CaseDashboardResponse
{
    public List<StatCardItem> Statistics { get; set; } = [];
    public List<StatusBreakdownItem> StatusOverview { get; set; } = [];
    public int TotalOpenCases { get; set; }
    public List<ChartDataPoint> ChartData { get; set; } = [];
    public string ResolutionRate { get; set; } = "+84.7%";
    public List<CaseItem> PriorityCases { get; set; } = [];
    public List<ActivityItem> RecentActivities { get; set; } = [];
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class CreateCaseRequest
{
    public string Category { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public int PeopleCount { get; set; } = 1;
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = "High";
    public string? Description { get; set; }
}

public class UpdateCaseStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ClaimCaseRequest
{
    public string? AssignedTo { get; set; } = "You";
}
