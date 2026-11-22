namespace CareLink.Application.DTOs;

public class PurchaseOrderDto
{
    public string Id { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public string Items { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string DeliveryDue { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePurchaseOrderRequest
{
    public string Supplier { get; set; } = string.Empty;
    public string Items { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "Standard";
    public string DeliveryDue { get; set; } = string.Empty;
    public string? Status { get; set; } = "Awaiting Approval";
    public string? Notes { get; set; }
}

public class UpdatePurchaseOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ProcurementFilterParams
{
    public string? Search { get; set; }
    public string? Supplier { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? Tab { get; set; }
}

public class ProcurementSummaryDto
{
    public int OpenOrders { get; set; }
    public int EmergencyOrders { get; set; }
    public int DueThisWeek { get; set; }
    public string SupplierFulfilment { get; set; } = "95.4%";
    public string TotalCommittedValue { get; set; } = "$186K";
    public int AwaitingApprovalCount { get; set; }
}
