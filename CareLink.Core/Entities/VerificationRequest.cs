namespace CareLink.Core.Entities;

public class VerificationDocument
{
    public string Id { get; set; } = string.Empty;
    public Guid? DocumentId { get; set; }
    public Guid? OrganizationId { get; set; }
    public string Category { get; set; } = "RegistrationCertificate";
    public string Name { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string Type { get; set; } = "PDF";
    public string ContentType { get; set; } = "application/pdf";
    public string Size { get; set; } = "1.2 MB";
    public long FileSizeBytes { get; set; }
    public string Status { get; set; } = "Uploaded";
    public bool CanDownload { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

public class VerificationRequest
{
    public string Id { get; set; } = string.Empty; // e.g. VER-2048
    public Guid? OrganizationId { get; set; }
    public string? UserId { get; set; }
    public string Applicant { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Partner Organisation, Partner Officer, Medical Partner, Resource Manager, Volunteer
    public string Evidence { get; set; } = string.Empty; // Summary string
    public string Priority { get; set; } = "Standard"; // High, Standard
    public string Status { get; set; } = "Pending Review"; // Pending Review, Additional Info, Approved, Rejected
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public List<VerificationDocument> Documents { get; set; } = new();

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
    public string? AreasOfOperation { get; set; }
    public string? AreasOfAssistance { get; set; }
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
    public string? AdditionalInfoRequest { get; set; }
    public string? AdditionalInfoResponse { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? DecisionRationale { get; set; }
}
