namespace CareLink.Application.DTOs;

public class MaintenanceRequestSummaryDto
{
    public int CriticalCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int PendingCount { get; set; }
    public int OnHoldCount { get; set; }
    public int TotalCount { get; set; }
}
