using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateCrisisEventDto
{
    [Required(ErrorMessage = "Crisis name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Crisis type is required.")]
    public string Type { get; set; } = "Flood";

    [Required(ErrorMessage = "Severity level is required.")]
    public string Severity { get; set; } = "Moderate · Level 2";

    [Required(ErrorMessage = "Operational area is required.")]
    public string Area { get; set; } = string.Empty;

    public string? StartDate { get; set; }

    [Required(ErrorMessage = "Assigned coordinator is required.")]
    public string Coordinator { get; set; } = string.Empty;

    public string? Status { get; set; } = "Active";

    public string? Description { get; set; }

    public long? AffectedPeopleCount { get; set; }

    public int? DistrictsCovered { get; set; }
}

public class UpdateCrisisEventDto
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Severity { get; set; }
    public string? Area { get; set; }
    public string? StartDate { get; set; }
    public string? Coordinator { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public long? AffectedPeopleCount { get; set; }
    public int? DistrictsCovered { get; set; }
}

public class UpdateCrisisStatusDto
{
    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = string.Empty;
}

public class CrisisEventQueryParameters
{
    public string? Search { get; set; }
    public string? Type { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
    public string? DateRange { get; set; }
    public bool? ShowArchived { get; set; }
}
