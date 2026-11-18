using CareLink.Application.Common;
using CareLink.Application.DTOs.Auth;
using CareLink.Application.DTOs.SecurityEvents;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareLink.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AuthService> _logger;
    private readonly ISecurityEventService _securityEventService;
    private readonly IPermissionService _permissionService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings,
        IMemoryCache memoryCache,
        ILogger<AuthService> logger,
        ISecurityEventService securityEventService,
        IPermissionService permissionService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
        _memoryCache = memoryCache;
        _logger = logger;
        _securityEventService = securityEventService;
        _permissionService = permissionService;
    }

    public async Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ApiResponse<RegisterResponseDto>.FailureResult(
                "An account with this email address already exists.");
        }

        var requestedRole = string.IsNullOrWhiteSpace(request.Role)
            ? UserRoles.Citizen
            : request.Role.Trim();

        // Security rule: Public registration must never allow self-assignment of privileged roles.
        if (UserRoles.IsPrivilegedRole(requestedRole))
        {
            return ApiResponse<RegisterResponseDto>.FailureResult(
                "Privileged administrative and coordination roles cannot be self-assigned during registration. Please register as a Citizen or contact your administrator.");
        }

        var assignedRole = UserRoles.IsValidRole(requestedRole) ? requestedRole : UserRoles.Citizen;

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            NIC = request.NIC?.Trim(),
            Address = request.Address?.Trim(),
            OrganizationId = request.OrganizationId,
            IsActive = true,
            EmailConfirmed = false, // Requires email confirmation before login
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToList();
            var errorMessage = errors.Count > 0
                ? string.Join("; ", errors)
                : "User registration failed.";
            return ApiResponse<RegisterResponseDto>.FailureResult(errorMessage, errors);
        }

        // Ensure role exists in the database
        if (!await _roleManager.RoleExistsAsync(assignedRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(assignedRole));
        }

        // Assign user to role
        await _userManager.AddToRoleAsync(user, assignedRole);

        // Generate email confirmation token
        var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = UrlTokenEncoder.Encode(rawToken);

        var baseUrl = !string.IsNullOrWhiteSpace(_emailSettings.VerificationBaseUrl)
            ? _emailSettings.VerificationBaseUrl
            : "http://localhost:5277/api/auth/verify-email";

        var confirmationUrl = $"{baseUrl}?email={Uri.EscapeDataString(user.Email)}&token={encodedToken}";

        _logger.LogInformation("Generated verification token for {Email}. URL: {ConfirmationUrl}", user.Email, confirmationUrl);

        // Dispatch verification email safely
        var emailSent = await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, confirmationUrl);

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = (await _permissionService.GetUserEffectivePermissionsAsync(user.Id)).ToList();

        var responseDto = new RegisterResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            EmailConfirmed = false,
            RequiresEmailVerification = true,
            Message = emailSent
                ? "Registration completed successfully. Please check your email to verify your account before logging in."
                : "Registration completed successfully. Please use the verification link sent to your email to activate your account.",
            User = MapToUserDto(user, roles, permissions)
        };

        return ApiResponse<RegisterResponseDto>.SuccessResult(
            responseDto, responseDto.Message);
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            await _securityEventService.CreateEventAsync(new CreateSecurityEventRequestDto
            {
                Type = "Repeated failed sign-in",
                User = request.Email,
                Source = "Web Portal",
                Severity = "Medium",
                Description = $"Failed sign-in attempt: Unrecognised account '{request.Email}'."
            });

            return ApiResponse<AuthResponseDto>.FailureResult("Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            await _securityEventService.CreateEventAsync(new CreateSecurityEventRequestDto
            {
                Type = "Repeated failed sign-in",
                User = user.Email ?? request.Email,
                Source = "Web Portal",
                Severity = "High",
                Description = $"Failed sign-in attempt: Incorrect password submitted for '{user.Email}'."
            });

            return ApiResponse<AuthResponseDto>.FailureResult("Invalid email or password.");
        }

        // 1. Enforce Email Verification Requirement
        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("Login rejected for unverified user: {Email}", request.Email);

            await _securityEventService.CreateEventAsync(new CreateSecurityEventRequestDto
            {
                Type = "Unusual account activity",
                User = user.Email ?? request.Email,
                Source = "Web Portal",
                Severity = "Low",
                Description = $"Authentication attempt on unverified account '{user.Email}'."
            });

            return ApiResponse<AuthResponseDto>.FailureResult(
                "Please verify your email before logging in. Check your inbox for the confirmation link.",
                new List<string> { "EMAIL_NOT_CONFIRMED" });
        }

        // 2. Enforce Account Active Status
        if (!user.IsActive)
        {
            await _securityEventService.CreateEventAsync(new CreateSecurityEventRequestDto
            {
                Type = "Unusual account activity",
                User = user.Email ?? request.Email,
                Source = "Web Portal",
                Severity = "Medium",
                Description = $"Deactivated user account '{user.Email}' attempted to authenticate."
            });

            return ApiResponse<AuthResponseDto>.FailureResult(
                "Your account has been deactivated. Please contact an administrator.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = (await _permissionService.GetUserEffectivePermissionsAsync(user.Id)).ToList();
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        var responseDto = new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = MapToUserDto(user, roles, permissions)
        };

        return ApiResponse<AuthResponseDto>.SuccessResult(
            responseDto, "Login successful.");
    }

    public async Task<ApiResponse<bool>> VerifyEmailAsync(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return ApiResponse<bool>.FailureResult("Email address and verification token are required.");
        }

        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user == null)
        {
            _logger.LogWarning("Email verification failed: User not found for '{Email}'", email);
            return ApiResponse<bool>.FailureResult("Invalid verification request or user does not exist.");
        }

        if (user.EmailConfirmed)
        {
            return ApiResponse<bool>.SuccessResult(true, "Your email is already verified. You can log in to your account.");
        }

        var rawToken = UrlTokenEncoder.SafeDecode(token);

        var confirmResult = await _userManager.ConfirmEmailAsync(user, rawToken);
        if (!confirmResult.Succeeded)
        {
            var errors = confirmResult.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Token confirmation failed for '{Email}': {Errors}", email, string.Join("; ", errors));

            return ApiResponse<bool>.FailureResult(
                "Email verification failed. The confirmation link is invalid, expired, or has already been used.",
                errors);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Email confirmed successfully for user: {Email}", email);

        return ApiResponse<bool>.SuccessResult(
            true, "Email has been successfully verified! You may now sign in to CareLink.");
    }

    public async Task<ApiResponse<bool>> ResendVerificationEmailAsync(string email)
    {
        const string genericResponse =
            "If your email address is registered and unconfirmed, a verification link has been sent to your inbox.";

        if (string.IsNullOrWhiteSpace(email))
        {
            return ApiResponse<bool>.FailureResult("Email address is required.");
        }

        var trimmedEmail = email.Trim();
        var normalizedEmail = trimmedEmail.ToUpperInvariant();

        // Rate limiting: 60-second cooldown per email address
        var cooldownKey = $"resend_cooldown_{normalizedEmail}";
        if (_memoryCache.TryGetValue(cooldownKey, out DateTime nextAllowedUtc))
        {
            var remainingSeconds = Math.Max(1, (int)(nextAllowedUtc - DateTime.UtcNow).TotalSeconds);
            return ApiResponse<bool>.FailureResult(
                $"A verification request was recently submitted. Please wait {remainingSeconds} seconds before requesting another.");
        }

        // Set 60-second cooldown in memory cache
        _memoryCache.Set(cooldownKey, DateTime.UtcNow.AddSeconds(60), TimeSpan.FromMinutes(2));

        var user = await _userManager.FindByEmailAsync(trimmedEmail);

        // Anti-enumeration protection: Return generic response if user doesn't exist or already verified
        if (user == null || user.EmailConfirmed)
        {
            _logger.LogInformation(
                "Resend verification requested for non-existent or already confirmed email: {Email}",
                trimmedEmail);
            return ApiResponse<bool>.SuccessResult(true, genericResponse);
        }

        // Generate new confirmation token
        var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = UrlTokenEncoder.Encode(rawToken);

        var baseUrl = !string.IsNullOrWhiteSpace(_emailSettings.VerificationBaseUrl)
            ? _emailSettings.VerificationBaseUrl
            : "http://localhost:5277/api/auth/verify-email";

        var confirmationUrl = $"{baseUrl}?email={Uri.EscapeDataString(user.Email!)}&token={encodedToken}";

        _logger.LogInformation("Resending verification token for {Email}. URL: {ConfirmationUrl}", user.Email, confirmationUrl);

        await _emailService.SendVerificationEmailAsync(user.Email!, user.FullName, confirmationUrl);

        return ApiResponse<bool>.SuccessResult(true, genericResponse);
    }

    public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<UserDto>.FailureResult("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = (await _permissionService.GetUserEffectivePermissionsAsync(user.Id)).ToList();
        return ApiResponse<UserDto>.SuccessResult(MapToUserDto(user, roles, permissions));
    }

    private static UserDto MapToUserDto(ApplicationUser user, IList<string> roles, IList<string>? permissions = null)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            NIC = user.NIC,
            Address = user.Address,
            Roles = roles,
            Permissions = permissions ?? new List<string>(),
            OrganizationId = user.OrganizationId,
            OrganizationName = user.Organization?.Name,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
