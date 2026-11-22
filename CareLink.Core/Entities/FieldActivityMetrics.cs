namespace CareLink.Core.Entities;

public class FieldActivityMetrics
{
    public int ActiveToday { get; set; } = 12;
    public int PeopleTargeted { get; set; } = 4840;
    public int PeopleReachedToday { get; set; } = 1864;
    public int TeamsDeployed { get; set; } = 18;
    public int StaffInField { get; set; } = 62;
    public int UpdatesOverdue { get; set; } = 2;
    public int TotalActivities { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
