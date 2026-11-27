using System.ComponentModel.DataAnnotations;
using CareLink.Core.Constants;

namespace CareLink.Application.DTOs.Auth;

public class RegisterDocumentUploadDto
{
    /// <summary>
    /// RegistrationCertificate, OrganisationProfile, AuthorisationLetter, SupportingDocument
    /// </summary>
    public string Category { get; set; } = "RegistrationCertificate";

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/pdf";

    public long FileSizeBytes { get; set; }

    public string Base64Content { get; set; } = string.Empty;
}

public class PartnerAdditionalInfoSubmissionDto
{
    [Required(ErrorMessage = "Login email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required to authenticate your submission.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please provide the requested additional information.")]
    public string ResponseText { get; set; } = string.Empty;

    public List<RegisterDocumentUploadDto>? AdditionalDocuments { get; set; }
}

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 150 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    public string? ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    public string PhoneNumber { get; set; } = string.Empty;

    public string? NIC { get; set; }

    public string? Address { get; set; }

    public string Role { get; set; } = UserRoles.Citizen;

    public Guid? OrganizationId { get; set; }

    // A. Partner Organisation Basic Information
    public string? OrganizationName { get; set; }
    public string? OrganizationType { get; set; }
    public string? OtherOrganizationType { get; set; }
    public string? RegistrationCountry { get; set; }
    public string? RegistrationNumber { get; set; }
    public int? YearEstablished { get; set; }
    public string? Website { get; set; }
    public string? OrganizationEmail { get; set; }
    public string? OrganizationPhone { get; set; }
    public string? SecondaryContactPhone { get; set; }
    public string? OrganizationAddress { get; set; }
    public string? District { get; set; }
    public string? Province { get; set; }
    public string? OrganizationDescription { get; set; }

    // B. Humanitarian Operational Information
    public List<string>? AreasOfOperation { get; set; }
    public List<string>? AreasOfAssistance { get; set; }
    public string? OtherAreaOfAssistance { get; set; }
    public string? OperationalCapacity { get; set; }
    public string? EmergencyResponseAvailability { get; set; }
    public string? AdditionalResources { get; set; }

    // C. Authorised Representative Information
    public string? RepresentativeFirstName { get; set; }
    public string? RepresentativeLastName { get; set; }
    public string? RepresentativeDesignation { get; set; }
    public string? RepresentativeEmail { get; set; }
    public string? RepresentativePhone { get; set; }
    public string? RepresentativeAlternativePhone { get; set; }

    // D. Step 3 Verification Documents & Confirmations
    public List<RegisterDocumentUploadDto>? Documents { get; set; }
    public bool? ConfirmAuthorisedRepresentative { get; set; }
    public bool? ConfirmAccurateInformation { get; set; }
    public bool? AgreeTermsAndPrivacy { get; set; }

    // Volunteer Profile Fields
    public string? Skills { get; set; }
    public string? Availability { get; set; }
    public string? PreferredLocation { get; set; }
}
