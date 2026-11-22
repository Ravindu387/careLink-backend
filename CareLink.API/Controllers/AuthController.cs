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
    /// Register a new user account and triggers email confirmation link delivery.
    /// Does not issue an authentication JWT until email verification is complete.
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
    /// Authenticate an existing user and generate a JWT bearer token.
    /// Requires that the user's email has been verified.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
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
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Verify a user's email address using the URL confirmation token.
    /// Supports both interactive browser requests (returns styled HTML) and API callers (returns JSON).
    /// </summary>
    [HttpGet("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
    {
        var result = await _authService.VerifyEmailAsync(email, token);

        // Content negotiation: Return styled HTML if requested by a web browser directly
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
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get the list of all valid platform user roles for registration or role assignment.
    /// </summary>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    public IActionResult GetAvailableRoles()
    {
        return Ok(ApiResponse<IReadOnlyList<string>>.SuccessResult(
            UserRoles.AllRoles, "Available platform roles retrieved successfully."));
    }
}
