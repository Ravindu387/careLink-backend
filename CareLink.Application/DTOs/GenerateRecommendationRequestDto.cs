using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class GenerateRecommendationRequestDto
{
    public string? RequestId { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public string Category { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string? Description { get; set; }

    public int? AffectedPeopleCount { get; set; }

    public bool HasVulnerableMembers { get; set; }

    public bool IsLifeThreatening { get; set; }

    public bool HasRoadBlockage { get; set; }

    public string? PriorityHint { get; set; }
}
