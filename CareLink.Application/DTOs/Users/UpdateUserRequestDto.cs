using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.Users;

public class UpdateUserRequestDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 150 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }

    public string? NIC { get; set; }

    public string? Address { get; set; }

    public string? Role { get; set; }

    public Guid? OrganizationId { get; set; }

    public string? OrganizationName { get; set; }

    public bool? IsActive { get; set; }

    public string? AccountStatus { get; set; }

    public string? Skills { get; set; }

    public string? Availability { get; set; }

    public string? PreferredLocation { get; set; }
}
