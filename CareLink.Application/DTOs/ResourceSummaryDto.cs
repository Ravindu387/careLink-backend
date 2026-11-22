namespace CareLink.Application.DTOs;

public class ResourceSummaryDto
{
    public int HealthyCount { get; set; }
    public int LowStockCount { get; set; }
    public int CriticalCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int ImmediateActionCount { get; set; }
    public int ExpiringSoonCount { get; set; }
    public int IncomingDonationsCount { get; set; } = 4;
    public int TotalItemsCount { get; set; }
}
