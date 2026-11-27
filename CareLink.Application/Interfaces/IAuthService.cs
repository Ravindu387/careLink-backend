using CareLink.Application.Common;
using CareLink.Application.DTOs.Auth;

namespace CareLink.Application.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Registers a new Citizen, Volunteer, or Partner Organization account according to role rules.
    /// Citizens and Volunteers become active immediately; Partner Organizations require Admin approval.
    /// </summary>
    Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// Authenticates an existing user using email and password, validating AccountStatus and IsActive.
    /// </summary>
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Allows a pending Partner Organization representative to submit additional information requested by an Administrator.
    /// </summary>
    Task<ApiResponse<bool>> SubmitPartnerAdditionalInfoAsync(PartnerAdditionalInfoSubmissionDto request);

    /// <summary>
    /// Completes account setup and sets password for an Admin-created user using a single-use token.
    /// </summary>
    Task<ApiResponse<bool>> SetupPasswordAsync(SetupPasswordRequestDto request);

    /// <summary>
    /// Validates whether a password setup token is valid, not expired, and has not been used.
    /// </summary>
    Task<ApiResponse<bool>> ValidateSetupTokenAsync(string email, string token);

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
    /// </summary>
    Task<ApiResponse<bool>> ResendVerificationEmailAsync(string email);
}
