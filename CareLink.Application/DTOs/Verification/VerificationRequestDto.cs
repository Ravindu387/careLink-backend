namespace CareLink.Application.DTOs.Verification;

public class VerificationRequestDto
{
    public string Id { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public string? UserId { get; set; }
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
    public string? OrganizationType { get; set; }
    public string? OtherOrganizationType { get; set; }
    public string? RegistrationCountry { get; set; }
    public string? RegistrationNumber { get; set; }
    public int? YearEstablished { get; set; }
    public string? Website { get; set; }
    public string? SecondaryContactPhone { get; set; }
    public string? Address { get; set; }
    public string? District { get; set; }
    public string? Province { get; set; }
    public string? Description { get; set; }

    public List<string> AreasOfOperation { get; set; } = new();
    public List<string> AreasOfAssistance { get; set; } = new();
    public string? OtherAreaOfAssistance { get; set; }
    public string? OperationalCapacity { get; set; }
    public string? EmergencyResponseAvailability { get; set; }
    public string? AdditionalResources { get; set; }

    public string? RepresentativeFullName { get; set; }
    public string? RepresentativeDesignation { get; set; }
    public string? RepresentativeEmail { get; set; }
    public string? RepresentativePhone { get; set; }
    public string? RepresentativeAlternativePhone { get; set; }
    public string? LoginEmail { get; set; }

    public string? ApprovalStatus { get; set; }
    public string? AdditionalInfoRequest { get; set; }
    public string? AdditionalInfoResponse { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? DecisionRationale { get; set; }
    public bool EmailSent { get; set; }
}
