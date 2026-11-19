namespace CareLink.Application.DTOs.Verification;

public class VerificationRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string Applicant { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Evidence { get; set; } = string.Empty;
    public string Priority { get; set; } = "Standard";
    public string Submitted { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; } = "Pending Review";
    public List<VerificationDocumentDto> Documents { get; set; } = new();

    public string? ApplicantEmail { get; set; }
    public string? ApplicantPhone { get; set; }
    public string? OrganizationName { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? DecisionRationale { get; set; }
}
