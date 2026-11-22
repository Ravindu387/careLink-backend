namespace CareLink.Core.Entities;

public class ResourceMetrics
{
    public int AverageAvailabilityPercent { get; set; } = 68;
    public int TotalWarehousesCount { get; set; } = 12;
    public int CriticalShortagesCount { get; set; } = 3;
    public string CriticalShortagesSummary { get; set; } = "Medical · transport";
    public int ActiveDispatchesCount { get; set; } = 27;
    public int ArrivingTodayDispatchesCount { get; set; } = 8;
    public int PendingRequestsCount { get; set; } = 43;
    public int HighPriorityPendingRequestsCount { get; set; } = 12;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
