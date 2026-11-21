namespace CareLink.Core.Entities;

public class PriorityQueueMetrics
{
    public int TotalCount { get; set; } = 1284;
    public int UnassignedCount { get; set; } = 63;
    public int OverdueCount { get; set; } = 12;
    public int PotentialDuplicatesCount { get; set; } = 9;
    public int CriticalOrOverdueRequiringAttentionCount { get; set; } = 12;
    public int CriticalCount { get; set; }
    public int HighCount { get; set; }
    public int MediumCount { get; set; }
    public int LowCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
