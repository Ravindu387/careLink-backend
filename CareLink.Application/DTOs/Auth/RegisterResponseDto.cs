namespace CareLink.Application.DTOs.Auth;

public class RegisterResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; } = false;
    public bool RequiresEmailVerification { get; set; } = true;
    public string Message { get; set; } = "Registration completed successfully. Please check your email to verify your account.";
    public UserDto? User { get; set; }
}
