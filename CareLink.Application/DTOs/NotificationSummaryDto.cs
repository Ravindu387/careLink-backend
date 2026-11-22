namespace CareLink.Application.DTOs;

public class NotificationSummaryDto
{
    public int UnreadCount { get; set; }
    public int CriticalCount { get; set; }
    public int RequestCount { get; set; }
    public int InventoryCount { get; set; }
    public int SystemCount { get; set; }
    public int FlagsCount { get; set; }
    public int VerificationCount { get; set; }
    public int CampaignsCount { get; set; }
    public int AllocationCount { get; set; }
    public int ExpenditureCount { get; set; }
    public int ReportsCount { get; set; }
    public int TotalCount { get; set; }

    // Citizen Portal Summary counts
    public int SafetyAlertsCount { get; set; }
    public int MyRequestsCount { get; set; }
}
