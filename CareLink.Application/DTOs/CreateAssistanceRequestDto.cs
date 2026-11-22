using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateAssistanceRequestDto
{
    [Required(ErrorMessage = "Category is required.")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location is required.")]
    public string Location { get; set; } = string.Empty;

    public string? CrisisEvent { get; set; } = "Monsoon Floods 2025";

    public string? Priority { get; set; }

    public string? Verification { get; set; }

    public string? Assigned { get; set; }

    public string? Organisation { get; set; }

    public string? Status { get; set; }

    public string? Description { get; set; }

    public int? AffectedPeopleCount { get; set; } = 1;

    public bool HasVulnerableMembers { get; set; }

    public bool IsLifeThreatening { get; set; }

    public string? ContactName { get; set; }

    public string? ContactPhone { get; set; }
}
