using CareLink.Application.Common;
using CareLink.Application.DTOs.Auth;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CareLink.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ApiResponse<AuthResponseDto>.FailureResult(
                "An account with this email address already exists.");
        }

        var assignedRole = string.IsNullOrWhiteSpace(request.Role)
            ? UserRoles.Citizen
            : request.Role.Trim();

        if (!UserRoles.IsValidRole(assignedRole))
        {
            return ApiResponse<AuthResponseDto>.FailureResult(
                $"Invalid role: '{request.Role}'. Valid roles are: {string.Join(", ", UserRoles.AllRoles)}");
        }

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
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToList();
            return ApiResponse<AuthResponseDto>.FailureResult(
                "User registration failed.", errors);
        }

        // Ensure role exists in the database
        if (!await _roleManager.RoleExistsAsync(assignedRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(assignedRole));
        }

        // Assign user to role
        await _userManager.AddToRoleAsync(user, assignedRole);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        var responseDto = new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = MapToUserDto(user, roles)
        };

        return ApiResponse<AuthResponseDto>.SuccessResult(
            responseDto, "Registration completed successfully.");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return ApiResponse<AuthResponseDto>.FailureResult("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return ApiResponse<AuthResponseDto>.FailureResult(
                "Your account has been deactivated. Please contact an administrator.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return ApiResponse<AuthResponseDto>.FailureResult("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        var responseDto = new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = MapToUserDto(user, roles)
        };

        return ApiResponse<AuthResponseDto>.SuccessResult(
            responseDto, "Login successful.");
    }

    public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<UserDto>.FailureResult("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return ApiResponse<UserDto>.SuccessResult(MapToUserDto(user, roles));
    }

    private static UserDto MapToUserDto(ApplicationUser user, IList<string> roles)
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
            OrganizationId = user.OrganizationId,
            OrganizationName = user.Organization?.Name,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
