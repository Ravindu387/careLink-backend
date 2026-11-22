using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class RecordDecisionRequestDto
{
    [Required(ErrorMessage = "Decision is required.")]
    public string Decision { get; set; } = string.Empty;

    [Required(ErrorMessage = "Rationale is required to maintain auditability.")]
    [MinLength(3, ErrorMessage = "Rationale must be at least 3 characters long.")]
    public string Rationale { get; set; } = string.Empty;

    public string? ReviewedBy { get; set; }
}
