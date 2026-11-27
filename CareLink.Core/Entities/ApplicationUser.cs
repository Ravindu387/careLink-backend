using CareLink.Core.Constants;
using Microsoft.AspNetCore.Identity;

namespace CareLink.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? NIC { get; set; }
    public string? Address { get; set; }
    public string? Designation { get; set; }
    public string? AlternativePhoneNumber { get; set; }
    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public bool IsActive { get; set; } = true;
    public string AccountStatus { get; set; } = AccountStatuses.Active;
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? Skills { get; set; }
    public string? Availability { get; set; }
    public string? PreferredLocation { get; set; }
    public bool RequiresPasswordSetup { get; set; } = false;
    public bool PasswordSetupTokenUsed { get; set; } = false;
    public DateTime? PasswordSetupTokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
