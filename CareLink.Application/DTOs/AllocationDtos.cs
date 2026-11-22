namespace CareLink.Application.DTOs;

public class AllocationDto
{
    public string Id { get; set; } = string.Empty;
    public string Request { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public string ApprovedAt { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateAllocationRequest
{
    public string Request { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Source { get; set; } = "Central Hub";
    public string Destination { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
}

public class UpdateAllocationStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
}

public class AllocationSummaryDto
{
    public int ActiveCount { get; set; }
    public int AwaitingCount { get; set; }
    public int CompletedCount { get; set; }
    public int ExceptionCount { get; set; }
}
