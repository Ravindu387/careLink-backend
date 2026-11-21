using CareLink.Application.Common;
using CareLink.Application.DTOs.Auth;

namespace CareLink.Application.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Registers a new user account, marks email as unconfirmed, generates confirmation token,
    /// and triggers verification email delivery. Does NOT issue an authentication JWT.
    /// </summary>
    Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// Authenticates an existing user and generates a JWT access token only if EmailConfirmed is true.
    /// </summary>
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Retrieves user profile details by identifier.
    /// </summary>
    Task<ApiResponse<UserDto>> GetCurrentUserAsync(string userId);

    /// <summary>
    /// Validates confirmation token and confirms the user's email address.
    /// </summary>
    Task<ApiResponse<bool>> VerifyEmailAsync(string email, string token);

    /// <summary>
    /// Generates and sends a new confirmation token if the email is registered and unconfirmed.
    /// Employs rate limiting and returns a generic response to prevent user enumeration.
    /// </summary>
    Task<ApiResponse<bool>> ResendVerificationEmailAsync(string email);
}
