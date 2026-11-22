using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.Auth;

public class ResendVerificationRequestDto
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;
}
