using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.Verification;

public class VerificationDecisionRequestDto
{
    [Required]
    public string Status { get; set; } = string.Empty; // Approved, Rejected, Additional Info

    [Required]
    public string Rationale { get; set; } = string.Empty;
}
