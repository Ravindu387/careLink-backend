namespace CareLink.Application.DTOs;

public class DispatchDto
{
    public string Id { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Vehicle { get; set; } = string.Empty;
    public string Driver { get; set; } = string.Empty;
    public string Departed { get; set; } = string.Empty;
    public string Eta { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CurrentLocation { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDispatchRequest
{
    public string From { get; set; } = "Central Hub";
    public string To { get; set; } = string.Empty;
    public string? Route { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Vehicle { get; set; } = string.Empty;
    public string Driver { get; set; } = string.Empty;
    public string? Departed { get; set; }
    public string Eta { get; set; } = string.Empty;
    public string? Status { get; set; } = "Pending";
    public string? Notes { get; set; }
}

public class UpdateDispatchStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? CurrentLocation { get; set; }
    public string? Notes { get; set; }
}

public class DispatchFilterParams
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Route { get; set; }
    public string? Vehicle { get; set; }
    public string? Tab { get; set; }
}

public class DispatchSummaryDto
{
    public int Active { get; set; }
    public int OnSchedule { get; set; }
    public int Delayed { get; set; }
    public int Delivered { get; set; }
    public int Total { get; set; }
    public int NeedsAction { get; set; }
}
