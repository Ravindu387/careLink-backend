using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class ResourceSharingCommitmentDto
{
    public string Id { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Partner { get; set; } = string.Empty;
    public string Scheduled { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactPerson { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CreateResourceSharingCommitmentDto
{
    [Required]
    [StringLength(150)]
    public string Resource { get; set; } = string.Empty;

    [Required]
    [Range(1, 10000000)]
    public int Quantity { get; set; } = 100;

    [Required]
    [StringLength(150)]
    public string Source { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Destination { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Partner { get; set; } = "Global Relief Network";

    [StringLength(100)]
    public string Scheduled { get; set; } = "Not scheduled";

    [StringLength(50)]
    public string Status { get; set; } = "Pending Approval";

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = "Food";

    [Required]
    [StringLength(100)]
    public string Location { get; set; } = "North District";

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? ContactPerson { get; set; }
}

public class UpdateResourceSharingCommitmentDto
{
    [StringLength(150)]
    public string? Resource { get; set; }

    [Range(1, 10000000)]
    public int? Quantity { get; set; }

    [StringLength(150)]
    public string? Source { get; set; }

    [StringLength(150)]
    public string? Destination { get; set; }

    [StringLength(150)]
    public string? Partner { get; set; }

    [StringLength(100)]
    public string? Scheduled { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [StringLength(50)]
    public string? Type { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? ContactPerson { get; set; }
}

public class UpdateResourceSharingStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public class ResourceSharingMetricsDto
{
    public int ActiveCommitments { get; set; }
    public string ActiveCommitmentsSubtitle { get; set; } = string.Empty;
    public string UnitsCommitted { get; set; } = string.Empty;
    public string UnitsCommittedSubtitle { get; set; } = string.Empty;
    public string DeliveredThisWeek { get; set; } = string.Empty;
    public string DeliveredThisWeekSubtitle { get; set; } = string.Empty;
    public int Exceptions { get; set; }
    public string ExceptionsSubtitle { get; set; } = string.Empty;

    public int TotalCommitments { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}

public class ResourceSharingQueryParameters
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
    public string? Partner { get; set; }
    public string? Status { get; set; }
}
