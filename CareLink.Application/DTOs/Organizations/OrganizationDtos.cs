using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.Organizations;

public class OrganizationDocumentDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileSizeDisplay { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public class OrganizationDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? OtherOrganizationType { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string RegistrationCountry { get; set; } = "Sri Lanka";
    public string? RegistrationNumber { get; set; }
    public int? YearEstablished { get; set; }
    public string? Website { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
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

    public string? RepresentativeUserId { get; set; }
    public string? RepresentativeFirstName { get; set; }
    public string? RepresentativeLastName { get; set; }
    public string? RepresentativeFullName { get; set; }
    public string? RepresentativeDesignation { get; set; }
    public string? RepresentativeEmail { get; set; }
    public string? RepresentativePhone { get; set; }
    public string? RepresentativeAlternativePhone { get; set; }

    public int MembersCount { get; set; }
    public string VerificationStatus { get; set; } = "Verified"; // Verified | Pending Review | Additional Info | Rejected
    public string Status { get; set; } = "Active"; // Active | Pending | Suspended | Rejected
    public string ApprovalStatus { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
    public string? RejectionReason { get; set; }
    public string? AdditionalInfoRequest { get; set; }
    public string? AdditionalInfoResponse { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool EmailSent { get; set; }

    public List<OrganizationDocumentDto> Documents { get; set; } = new();
}

public class OrganizationStatsDto
{
    public int TotalOrganizations { get; set; }
    public int ActiveOrganizations { get; set; }
    public int PendingOrganizations { get; set; }
    public int SuspendedOrganizations { get; set; }
    public int VerifiedOrganizations { get; set; }
    public int AdditionalInfoOrganizations { get; set; }
    public int RejectedOrganizations { get; set; }
}

public class OrganizationQueryDto
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CreateOrganizationRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Scope { get; set; } = "National";
    public string? RegistrationNumber { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? District { get; set; }
    public string? Province { get; set; }
    public string? Description { get; set; }
}

public class UpdateOrganizationRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? District { get; set; }
    public string? Province { get; set; }
    public string? Description { get; set; }
}

public class UpdateOrganizationStatusRequestDto
{
    public string Status { get; set; } = "Active"; // Active | Pending | Suspended
}

public class OrganizationReviewDecisionRequestDto
{
    /// <summary>
    /// Decision: Approve | Reject | RequestInfo (or Approved | Rejected | Additional Info)
    /// </summary>
    [Required(ErrorMessage = "Decision is required.")]
    public string Decision { get; set; } = "Approve";

    [Required(ErrorMessage = "Decision rationale or reason is required.")]
    public string Rationale { get; set; } = string.Empty;
}
