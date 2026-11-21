namespace CareLink.Application.DTOs;

public class StockTransferDto
{
    public string Id { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string Scheduled { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateStockTransferRequest
{
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string Scheduled { get; set; } = string.Empty;
}

public class UpdateStockTransferStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class StockTransferSummaryDto
{
    public int ActiveCount { get; set; }
    public int AwaitingCount { get; set; }
    public int CompletedCount { get; set; }
    public int ExceptionCount { get; set; }
    public int TotalCount { get; set; }
}
