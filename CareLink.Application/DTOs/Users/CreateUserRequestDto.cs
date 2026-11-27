using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.Users;

public class CreateUserRequestDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 150 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }

    public string? NIC { get; set; }

    public string? Address { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = string.Empty;

    public Guid? OrganizationId { get; set; }

    public string? OrganizationName { get; set; }

    public string? Skills { get; set; }

    public string? Availability { get; set; }

    public string? PreferredLocation { get; set; }

    public bool IsActive { get; set; } = true;
}
