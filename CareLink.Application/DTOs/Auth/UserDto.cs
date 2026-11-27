using CareLink.Core.Constants;

namespace CareLink.Application.DTOs.Auth;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AlternativePhoneNumber { get; set; }
    public string? Designation { get; set; }
    public string? NIC { get; set; }
    public string? Address { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public IList<string> Permissions { get; set; } = new List<string>();
    public Guid? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public string? OrganizationApprovalStatus { get; set; }
    public string? AdditionalInfoRequest { get; set; }
    public bool IsActive { get; set; }
    public string AccountStatus { get; set; } = AccountStatuses.Active;
    public string? RejectionReason { get; set; }
    public string? Skills { get; set; }
    public string? Availability { get; set; }
    public string? PreferredLocation { get; set; }
    public bool RequiresPasswordSetup { get; set; }
    public DateTime CreatedAt { get; set; }
}
