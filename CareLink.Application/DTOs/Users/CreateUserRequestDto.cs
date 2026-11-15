using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.Users;

public class CreateUserRequestDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? NIC { get; set; }

    public string? Address { get; set; }

    [Required]
    public string Role { get; set; } = string.Empty;

    public Guid? OrganizationId { get; set; }

    public bool IsActive { get; set; } = true;
}
