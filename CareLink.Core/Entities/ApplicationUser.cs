using Microsoft.AspNetCore.Identity;

namespace CareLink.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? NIC { get; set; }
    public string? Address { get; set; }
    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
