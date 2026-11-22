using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class OperationPointDto
{
    public string Id { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Type { get; set; } = "Request";
    public string Priority { get; set; } = "Medium";
    public int People { get; set; }
    public int Confidence { get; set; }
    public string NearestShelter { get; set; } = string.Empty;
    public string AssignedTeam { get; set; } = "Unassigned";
    public string Status { get; set; } = "Active";
    public string Time { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public int? Capacity { get; set; }
    public int? CurrentOccupancy { get; set; }
    public string? SuppliesStatus { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class LiveOperationsMapMetricsDto
{
    public int TotalPoints { get; set; }
    public int RequestCount { get; set; }
    public int ShelterCount { get; set; }
    public int WarehouseCount { get; set; }
    public int TeamCount { get; set; }
    public int CriticalCount { get; set; }
    public int HighCount { get; set; }
    public int MediumCount { get; set; }
    public int TotalPeopleAffected { get; set; }
    public IReadOnlyList<string> Districts { get; set; } = new List<string>();
    public IReadOnlyList<string> Layers { get; set; } = new List<string>();
    public DateTimeOffset LastSyncedAt { get; set; }
}

public class OperationPointQueryParameters
{
    public string? Search { get; set; }
    public string? District { get; set; }
    public string? Layer { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
}

public class CreateOperationPointDto
{
    [Required]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string District { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Request|Shelter|Warehouse|Team)$", ErrorMessage = "Type must be 'Request', 'Shelter', 'Warehouse', or 'Team'.")]
    public string Type { get; set; } = "Request";

    [RegularExpression("^(Critical|High|Medium|Low)$", ErrorMessage = "Priority must be 'Critical', 'High', 'Medium', or 'Low'.")]
    public string Priority { get; set; } = "Medium";

    [Range(0, 50000)]
    public int People { get; set; } = 0;

    [Range(1, 100)]
    public int Confidence { get; set; } = 90;

    public string? NearestShelter { get; set; }

    public string? AssignedTeam { get; set; }

    public string? Status { get; set; }

    [Range(0.0, 100.0)]
    public double X { get; set; } = 50.0;

    [Range(0.0, 100.0)]
    public double Y { get; set; } = 50.0;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public int? Capacity { get; set; }
    public string? SuppliesStatus { get; set; }
    public string? Notes { get; set; }
}

public class UpdateOperationPointDto
{
    [StringLength(120)]
    public string? Title { get; set; }

    [StringLength(80)]
    public string? District { get; set; }

    [StringLength(160)]
    public string? Location { get; set; }

    [RegularExpression("^(Request|Shelter|Warehouse|Team)$", ErrorMessage = "Type must be 'Request', 'Shelter', 'Warehouse', or 'Team'.")]
    public string? Type { get; set; }

    [RegularExpression("^(Critical|High|Medium|Low)$", ErrorMessage = "Priority must be 'Critical', 'High', 'Medium', or 'Low'.")]
    public string? Priority { get; set; }

    [Range(0, 50000)]
    public int? People { get; set; }

    [Range(1, 100)]
    public int? Confidence { get; set; }

    public string? NearestShelter { get; set; }
    public string? AssignedTeam { get; set; }
    public string? Status { get; set; }

    [Range(0.0, 100.0)]
    public double? X { get; set; }

    [Range(0.0, 100.0)]
    public double? Y { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public int? Capacity { get; set; }
    public int? CurrentOccupancy { get; set; }
    public string? SuppliesStatus { get; set; }
    public string? Notes { get; set; }
}

public class AssignOperationPointDto
{
    [Required]
    public string AssignedTeam { get; set; } = string.Empty;

    public string? Status { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCoordinatesDto
{
    [Range(0.0, 100.0)]
    public double X { get; set; }

    [Range(0.0, 100.0)]
    public double Y { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
