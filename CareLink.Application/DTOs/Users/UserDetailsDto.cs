using CareLink.Core.Constants;

namespace CareLink.Application.DTOs.Users;

public class UserDetailsDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? NIC { get; set; }
    public string? Address { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public IList<string> Permissions { get; set; } = new List<string>();
    public string Role => Roles.FirstOrDefault() ?? "Citizen";
    public Guid? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public string? OrganizationType { get; set; }
    public string? RegistrationNumber { get; set; }
    public bool IsActive { get; set; }
    public string AccountStatus { get; set; } = AccountStatuses.Active;
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? Skills { get; set; }
    public string? Availability { get; set; }
    public string? PreferredLocation { get; set; }
    public bool RequiresPasswordSetup { get; set; }
    public bool EmailSent { get; set; }
    public string? EmailDeliveryStatus { get; set; }
    public string? EmailFailureCategory { get; set; }
    public string? EmailFailureDetail { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string LastActive { get; set; } = "Never";
}
