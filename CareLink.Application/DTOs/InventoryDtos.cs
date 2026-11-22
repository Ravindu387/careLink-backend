namespace CareLink.Application.DTOs;

public class InventoryItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public int OnHand { get; set; }
    public int Reserved { get; set; }
    public int InTransit { get; set; }
    public int Available { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateInventoryItemRequest
{
    public string Resource { get; set; } = string.Empty;
    public string Category { get; set; } = "Food & Water";
    public string Warehouse { get; set; } = "Central Logistics Hub";
    public int OnHand { get; set; } = 100;
    public int Reserved { get; set; } = 0;
    public int InTransit { get; set; } = 0;
    public string Unit { get; set; } = "units";
    public DateTime? ExpiryDate { get; set; }
}

public class UpdateInventoryItemRequest
{
    public string? Resource { get; set; }
    public string? Category { get; set; }
    public string? Warehouse { get; set; }
    public int? OnHand { get; set; }
    public int? Reserved { get; set; }
    public int? InTransit { get; set; }
    public string? Status { get; set; }
    public string? Unit { get; set; }
}

public class AdjustStockRequest
{
    public int QuantityDelta { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class InventoryFilterParams
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? Warehouse { get; set; }
    public string? Status { get; set; }
    public string? Tab { get; set; }
}

public class InventorySummaryDto
{
    public int TotalSkus { get; set; }
    public int LowStock { get; set; }
    public int OutOfStock { get; set; }
    public int ExpiringSoon { get; set; }
    public int TotalAvailable { get; set; }
}
