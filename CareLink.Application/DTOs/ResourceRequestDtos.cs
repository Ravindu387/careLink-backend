namespace CareLink.Application.DTOs;

public class ResourceRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Category { get; set; } = "Food & Water";
    public string Source { get; set; } = "Pending Assignment";
    public string Age { get; set; } = "Just now";
    public string Status { get; set; } = "Awaiting Approval";
    public DateTime CreatedAt { get; set; }
}

public class CreateResourceRequestPayload
{
    public string Unit { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Quantity { get; set; } = "100 units";
    public string Priority { get; set; } = "Medium";
    public string Category { get; set; } = "Food & Water";
    public string? Source { get; set; }
}

public class UpdateResourceRequestStatusPayload
{
    public string Status { get; set; } = "Awaiting Approval";
    public string? Source { get; set; }
}

public class ResourceRequestSummaryDto
{
    public int TotalCount { get; set; }
    public int OpenCount { get; set; }
    public int AwaitingApprovalCount { get; set; }
    public int PartiallyAvailableCount { get; set; }
    public int FulfilledTodayCount { get; set; }
    public int NeedsActionCount { get; set; }
}

public class ResourceRequestFilterParams
{
    public string? Tab { get; set; }
    public string? Search { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? Requester { get; set; }
    public string? Status { get; set; }
}
