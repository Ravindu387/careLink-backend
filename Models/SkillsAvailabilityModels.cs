namespace CareLink.Backend.Models;

public class SkillCoverageItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Verified { get; set; }
    public int Available { get; set; }
    public int Percentage { get; set; }
    public string Category { get; set; } = "Operations";
}

public class AvailabilityForecastDto
{
    public string Day { get; set; } = string.Empty;
    public int Volunteers { get; set; }
}

public class SkillsAvailabilityStatsDto
{
    public int AvailableToday { get; set; } = 642;
    public string AvailableTodayPercentage { get; set; } = "26.8% of volunteers";
    public int AvailableThisWeek { get; set; } = 1184;
    public string AvailableThisWeekSub { get; set; } = "Across all areas";
    public int CriticalSkillGaps { get; set; } = 3;
    public string CriticalSkillGapsSummary { get; set; } = "Rescue, nursing, drivers";
    public string SchedulesUpdatedRate { get; set; } = "89%";
    public string SchedulesUpdatedSub { get; set; } = "Within last 7 days";
}

public class CoverageWarningDto
{
    public string Title { get; set; } = "Thursday coverage gap";
    public string Description { get; set; } = "Additional rescue drivers needed from 18:00–22:00.";
    public string Severity { get; set; } = "Warning";
}

public class SkillsAvailabilityOverviewDto
{
    public SkillsAvailabilityStatsDto Stats { get; set; } = new();
    public List<SkillCoverageItemDto> Skills { get; set; } = [];
    public List<AvailabilityForecastDto> Forecast { get; set; } = [];
    public CoverageWarningDto Warning { get; set; } = new();
    public string OperationalArea { get; set; } = "Greater Riverton";
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class CreateSkillRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Operations";
}

public class AvailabilityRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string Skill { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int Volunteers { get; set; } = 5;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
