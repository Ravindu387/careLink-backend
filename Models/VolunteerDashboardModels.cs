namespace CareLink.Backend.Models;

public class DashboardStatItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Color { get; set; } = "blue"; // blue, green, amber, red
}

public class DashboardSkillAvailabilityDto
{
    public string Name { get; set; } = string.Empty;
    public int Available { get; set; }
    public int Percentage { get; set; }
}

public class WeeklyTaskChartItemDto
{
    public string Day { get; set; } = string.Empty;
    public int Created { get; set; }
    public int Completed { get; set; }
}

public class DashboardUrgentTaskDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public string Priority { get; set; } = "High"; // Critical, High, Medium
    public string Schedule { get; set; } = string.Empty;
    public string Filled { get; set; } = "0 / 1";
    public string Status { get; set; } = "Open"; // In Progress, Assigned, Open
}

public class DashboardActivityItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Type { get; set; } = "checkin"; // checkin, ai_match, verification, replacement, general
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class VolunteerDashboardOverviewDto
{
    public List<DashboardStatItemDto> Stats { get; set; } = [];
    public List<DashboardSkillAvailabilityDto> Skills { get; set; } = [];
    public List<WeeklyTaskChartItemDto> WeeklyData { get; set; } = [];
    public List<DashboardUrgentTaskDto> UrgentTasks { get; set; } = [];
    public List<DashboardActivityItemDto> RecentActivities { get; set; } = [];
    public string OperationsAlert { get; set; } = "Realistic sample records for the Greater Riverton flood response.";
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class CreateDashboardActivityRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "general";
}
