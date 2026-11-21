using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CollaborationRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Partner { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Support { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public class CreateCollaborationRequestDto
{
    [Required(ErrorMessage = "Title is required.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Requesting Partner is required.")]
    public string Partner { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sector is required.")]
    public string Sector { get; set; } = "WASH";

    [Required(ErrorMessage = "Operational Area is required.")]
    public string Area { get; set; } = "North Ward";

    [Required(ErrorMessage = "Required support description is mandatory.")]
    public string Support { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start Date is required.")]
    public string StartDate { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}

public class UpdateCollaborationRequestDto
{
    public string? Title { get; set; }
    public string? Partner { get; set; }
    public string? Sector { get; set; }
    public string? Area { get; set; }
    public string? Support { get; set; }
    public string? StartDate { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}

public class UpdateCollaborationStatusDto
{
    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = string.Empty;
}

public class CollaborationRequestMetricsDto
{
    public int OpenCollaborations { get; set; }
    public int AwaitingResponse { get; set; }
    public int ActivePartnerships { get; set; }
    public int CompletedThisMonth { get; set; }
    public int TotalCollaborations { get; set; }
    public int SectorsCount { get; set; }
    public int PartnersCount { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}

public class CollaborationQueryParameters
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? Sector { get; set; }
    public string? Partner { get; set; }
    public string? Area { get; set; }
    public string? Status { get; set; }
}
