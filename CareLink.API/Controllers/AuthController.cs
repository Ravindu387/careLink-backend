using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Auth;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using CareLink.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Public self-registration for Citizen, Volunteer, and Partner Organization accounts.
    /// Citizens and Volunteers become active immediately; Partner Organizations enter PendingApproval status.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse<RegisterResponseDto>.FailureResult("Validation failed.", errors));
        }

        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Authenticate an existing user using email and password and generate a JWT bearer token.
    /// Enforces AccountStatus (Active vs PendingApproval, Rejected, Disabled) and password setup requirements.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse<AuthResponseDto>.FailureResult("Validation failed.", errors));
        }

        var result = await _authService.LoginAsync(request);
        if (!result.Success)
        {
            if (result.Errors != null &&
                (result.Errors.Contains("ACCOUNT_PENDING_APPROVAL") ||
                 result.Errors.Contains("ACCOUNT_REJECTED") ||
                 result.Errors.Contains("ACCOUNT_DISABLED") ||
                 result.Errors.Contains("PASSWORD_SETUP_REQUIRED")))
            {
                return StatusCode(StatusCodes.Status403Forbidden, result);
            }

            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Submit additional verification information requested by an Administrator for a pending Partner Organisation.
    /// </summary>
    [HttpPost("partner-additional-info")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitPartnerAdditionalInfo([FromBody] PartnerAdditionalInfoSubmissionDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse<bool>.FailureResult("Validation failed.", errors));
        }

        var result = await _authService.SubmitPartnerAdditionalInfoAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Validate an account activation / password setup token for an Admin-created user.
    /// </summary>
    [HttpGet("verify-setup-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifySetupToken([FromQuery] string email, [FromQuery] string token)
    {
        var result = await _authService.ValidateSetupTokenAsync(email, token);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Complete account setup and set password using a single-use token sent to an Admin-created user.
    /// </summary>
    [HttpPost("setup-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetupPassword([FromBody] SetupPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse<bool>.FailureResult("Validation failed.", errors));
        }

        var result = await _authService.SetupPasswordAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Verify a user's email address using the URL confirmation token.
    /// </summary>
    [HttpGet("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
    {
        var result = await _authService.VerifyEmailAsync(email, token);

        var isHtmlPreferred = Request.Headers.Accept.ToString().Contains("text/html");

        if (result.Success)
        {
            if (isHtmlPreferred)
            {
                var html = EmailTemplates.GetVerificationSuccessHtml("http://localhost:5173/login");
                return Content(html, "text/html");
            }
            return Ok(result);
        }

        if (isHtmlPreferred)
        {
            var html = EmailTemplates.GetVerificationErrorHtml(
                result.Message,
                "http://localhost:5173/login");
            return Content(html, "text/html");
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Request a new email verification link with rate-limiting and anti-enumeration protections.
    /// </summary>
    [HttpPost("resend-verification")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse<bool>.FailureResult("Validation failed.", errors));
        }

        var result = await _authService.ResendVerificationEmailAsync(request.Email);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieve the currently authenticated user's profile and assigned roles.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<UserDto>.FailureResult("User identifier not found in token claims."));
        }

        var result = await _authService.GetCurrentUserAsync(userId);
        if (!result.Success)
        {
            if (result.Message.Contains("not active", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, result);
            }
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get the list of all valid platform user roles.
    /// </summary>
    [HttpGet("roles")]
    public IActionResult GetAvailableRoles()
    {
        return Ok(ApiResponse<IReadOnlyList<string>>.SuccessResult(
            UserRoles.AllRoles, "Available platform roles retrieved successfully."));
    }

    /// <summary>
    /// Get the list of roles permitted for public self-registration.
    /// </summary>
    [HttpGet("public-roles")]
    [AllowAnonymous]
    public IActionResult GetPublicRegistrationRoles()
    {
        return Ok(ApiResponse<IReadOnlyList<string>>.SuccessResult(
            UserRoles.PublicRegistrationRoles, "Public registration roles retrieved successfully."));
    }
}
