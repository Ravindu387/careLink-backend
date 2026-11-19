using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.Verification;

public class CreateVerificationRequestDto
{
    [Required]
    public string Applicant { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    public string Priority { get; set; } = "Standard"; // High, Standard

    public string? ApplicantEmail { get; set; }
    public string? ApplicantPhone { get; set; }
    public string? OrganizationName { get; set; }

    public List<string> DocumentNames { get; set; } = new();
}
