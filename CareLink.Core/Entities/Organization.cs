using CareLink.Core.Constants;

namespace CareLink.Core.Entities;

public class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // A. Organisation Basic Information
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? OtherOrganizationType { get; set; }
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

    // B. Humanitarian Operational Information
    public string? AreasOfOperation { get; set; }
    public string? AreasOfAssistance { get; set; }
    public string? OtherAreaOfAssistance { get; set; }
    public string? OperationalCapacity { get; set; }
    public string? EmergencyResponseAvailability { get; set; }
    public string? AdditionalResources { get; set; }

    // C. Authorised Representative Information
    public string? RepresentativeUserId { get; set; }
    public string? RepresentativeFirstName { get; set; }
    public string? RepresentativeLastName { get; set; }
    public string? RepresentativeFullName { get; set; }
    public string? RepresentativeDesignation { get; set; }
    public string? RepresentativeEmail { get; set; }
    public string? RepresentativePhone { get; set; }
    public string? RepresentativeAlternativePhone { get; set; }

    // D. Verification & Declarations
    public bool ConfirmedAuthorisedRepresentative { get; set; }
    public bool ConfirmedAccurateInformation { get; set; }
    public bool AgreedToTermsAndPrivacy { get; set; }

    // E. Approval & Audit Status
    public bool IsActive { get; set; } = true;
    public string ApprovalStatus { get; set; } = AccountStatuses.Active;
    public string? RejectionReason { get; set; }
    public string? AdditionalInfoRequest { get; set; }
    public string? AdditionalInfoResponse { get; set; }
    public DateTime? AdditionalInfoRequestedAt { get; set; }
    public DateTime? AdditionalInfoSubmittedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<OrganizationDocument> Documents { get; set; } = new List<OrganizationDocument>();
}
