using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.AiRecommendations;

public class RecordDecisionRequestDto
{
    [Required(ErrorMessage = "Selected priority is required.")]
    public string Priority { get; set; } = string.Empty;

    [Required(ErrorMessage = "Decision rationale is required.")]
    [MinLength(10, ErrorMessage = "Rationale must be at least 10 characters long.")]
    public string Rationale { get; set; } = string.Empty;
}
