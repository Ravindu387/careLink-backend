namespace CareLink.Application.DTOs;

public class DashboardStatDto
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = "blue";
}

public class DashboardCriticalAlertDto
{
    public int ShortagesCount { get; set; } = 3;
    public string Title { get; set; } = "3 Critical shortages";
    public string Description { get; set; } = "Medical kits and fuel require immediate action";
    public int PendingRequestsCount { get; set; } = 18;
    public int DistrictsCount { get; set; } = 4;
}

public class DashboardStockMovementDto
{
    public string Day { get; set; } = string.Empty;
    public int Received { get; set; }
    public int Issued { get; set; }
}

public class DashboardStockHealthDto
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Color { get; set; } = "bg-sky-600";
}

public class DashboardRequestItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Priority { get; set; } = "Critical";
    public string Category { get; set; } = string.Empty;
    public string Source { get; set; } = "Central Hub";
    public string Age { get; set; } = "Just now";
    public string Status { get; set; } = "Awaiting Approval";
    public DateTime CreatedAt { get; set; }
}

public class DashboardActivityDto
{
    public string Icon { get; set; } = "Package";
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
}

public class DashboardQuickActionDto
{
    public string Icon { get; set; } = "Plus";
    public string Label { get; set; } = string.Empty;
    public string TargetPath { get; set; } = string.Empty;
    public string ActionType { get; set; } = "navigate";
}

public class ResourceDashboardDto
{
    public List<DashboardStatDto> Stats { get; set; } = [];
    public DashboardCriticalAlertDto CriticalAlert { get; set; } = new();
    public List<DashboardStockMovementDto> StockMovement { get; set; } = [];
    public List<DashboardStockHealthDto> StockHealth { get; set; } = [];
    public List<DashboardRequestItemDto> PriorityRequests { get; set; } = [];
    public List<DashboardActivityDto> RecentActivities { get; set; } = [];
    public List<DashboardQuickActionDto> QuickActions { get; set; } = [];
    public string PrimaryHub { get; set; } = "Central Logistics Hub";
    public string ActiveOperation { get; set; } = "Monsoon Floods 2025";
}

public class RecordStockReceiptRequest
{
    public string Resource { get; set; } = string.Empty;
    public string Category { get; set; } = "Food & Water";
    public int Quantity { get; set; }
    public string Warehouse { get; set; } = "Central Hub";
    public string? Supplier { get; set; }
    public string? ReceivedBy { get; set; } = "Daniel Perera";
    public string? Notes { get; set; }
}

public class CreateDashboardResourceRequestDto
{
    public string Unit { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Priority { get; set; } = "Critical";
    public string Category { get; set; } = "Food & Water";
    public string Source { get; set; } = "Central Hub";
}

public class ResourceRequestQueryParams
{
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
}
