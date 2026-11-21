using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class SharedCrisisEventDto
{
    public string Id { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Sectors { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Coordinator { get; set; } = string.Empty;
    public int DistrictsCovered { get; set; }
    public long AffectedPeopleCount { get; set; }
}

public class SharedCrisisMetricsDto
{
    public int SharedEvents { get; set; }
    public string SharedEventsSubtitle { get; set; } = string.Empty;
    public int MajorResponse { get; set; }
    public string MajorResponseSubtitle { get; set; } = string.Empty;
    public int DistrictsCovered { get; set; }
    public string DistrictsCoveredSubtitle { get; set; } = string.Empty;
    public int CoordinationSectors { get; set; }
    public string CoordinationSectorsSubtitle { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}

public class CreateCrisisAccessRequestDto
{
    [Required]
    [StringLength(150)]
    public string EventName { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Reason { get; set; } = string.Empty;

    public string? PartnerName { get; set; } = "Global Relief Network";
    public string? CrisisId { get; set; }
}

public class CrisisAccessRequestResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string CrisisId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset SubmittedAt { get; set; }
}

public class SharedCrisisQueryParameters
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? Type { get; set; }
    public string? Severity { get; set; }
    public string? Area { get; set; }
    public string? Status { get; set; }
}
