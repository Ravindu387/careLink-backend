namespace CareLink.Core.Entities;

public class LiveOperationsMapMetrics
{
    public int TotalPoints { get; set; }
    public int RequestCount { get; set; }
    public int ShelterCount { get; set; }
    public int WarehouseCount { get; set; }
    public int TeamCount { get; set; }
    public int CriticalCount { get; set; }
    public int HighCount { get; set; }
    public int MediumCount { get; set; }
    public int TotalPeopleAffected { get; set; }
    public IReadOnlyList<string> Districts { get; set; } = new List<string>();
    public IReadOnlyList<string> Layers { get; set; } = new List<string>();
    public DateTimeOffset LastSyncedAt { get; set; } = DateTimeOffset.UtcNow;
}
