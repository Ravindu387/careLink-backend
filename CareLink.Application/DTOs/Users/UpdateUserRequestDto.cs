using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.Users;

public class UpdateUserRequestDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? NIC { get; set; }

    public string? Address { get; set; }

    public string? Role { get; set; }

    public Guid? OrganizationId { get; set; }

    public bool? IsActive { get; set; }
}
