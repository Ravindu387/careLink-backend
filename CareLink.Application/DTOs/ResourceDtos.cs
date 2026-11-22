using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class ResourceMetricsDto
{
    public int AvailabilityPercent { get; set; }
    public int TotalWarehouses { get; set; }
    public int CriticalShortages { get; set; }
    public string CriticalShortagesSummary { get; set; } = "Medical · transport";
    public int ActiveDispatches { get; set; }
    public int ArrivingToday { get; set; }
    public int PendingRequests { get; set; }
    public int HighPriorityRequests { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}

public class CreateResourceRequestDto
{
    [Required]
    [StringLength(150)]
    public string ResourceName { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000000, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [Required]
    [StringLength(100)]
    public string OperationalArea { get; set; } = "Central Depot";

    [RegularExpression("^(Normal|High|Critical)$", ErrorMessage = "Priority must be Normal, High, or Critical.")]
    public string? Priority { get; set; } = "Normal";

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class ResourceRequestResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string OperationalArea { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public string Status { get; set; } = "Pending";
    public string RequestedBy { get; set; } = "Emergency Coordinator";
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? FulfilledAt { get; set; }
}

public class CreateResourceItemDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = "Food & Water";

    [Required]
    [StringLength(100)]
    public string Location { get; set; } = "Central Depot";

    [Range(0, 1000000)]
    public int Available { get; set; }

    [Range(0, 1000000)]
    public int Reserved { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; } = "Units";
}

public class UpdateResourceStockDto
{
    [Range(0, 1000000)]
    public int? Available { get; set; }

    [Range(0, 1000000)]
    public int? Reserved { get; set; }

    [RegularExpression("^(Healthy|Low Stock|Critical)$", ErrorMessage = "Status must be Healthy, Low Stock, or Critical.")]
    public string? Status { get; set; }
}
