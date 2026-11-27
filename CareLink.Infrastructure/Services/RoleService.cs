using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Roles;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using CareLink.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareLink.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        ApplicationDbContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IPermissionService permissionService,
        ILogger<RoleService> logger)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<ApiResponse<IReadOnlyList<RoleDto>>> GetRolesAsync()
    {
        var roles = await _roleManager.Roles.ToListAsync();

        var userCounts = await _context.UserRoles
            .GroupBy(ur => ur.RoleId)
            .Select(g => new { RoleId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoleId, x => x.Count);

        var claims = await _context.RoleClaims
            .Where(rc => rc.ClaimType == "Permission")
            .ToListAsync();

        var claimsByRole = claims
            .GroupBy(rc => rc.RoleId)
            .ToDictionary(g => g.Key, g => g.Select(c => c.ClaimValue!).ToHashSet(StringComparer.OrdinalIgnoreCase));

        var dtos = new List<RoleDto>();
        foreach (var role in roles)
        {
            var roleId = role.Id;
            var roleName = role.Name ?? string.Empty;
            userCounts.TryGetValue(roleId, out var uCount);

            int pCount;
            if (claimsByRole.TryGetValue(roleId, out var set) && set.Count > 0)
            {
                pCount = set.Count;
            }
            else
            {
                pCount = DefaultRolePermissions.GetDefaultPermissions(roleName).Count;
            }

            var isSystem = UserRoles.AllRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);

            var purpose = roleName switch
            {
                UserRoles.Admin => "Full platform administrator with unrestricted operational authority",
                UserRoles.SystemAdmin => "Core system administration, security, and infrastructure configuration",
                UserRoles.CaseManager => "Assistance request verification, priority triage, and case lifecycle management",
                UserRoles.EmergencyCoordinator => "Cross-sector crisis response, triage coordination, and emergency tasking",
                UserRoles.FundingOfficer => "Financial aid campaigns, donation oversight, and expenditure governance",
                UserRoles.PartnerOfficer => "Partner NGO/agency coordination, resource deployment, and verification",
                UserRoles.ResourceManager => "Supply chain logistics, warehouse inventory, and resource distribution",
                UserRoles.ShelterAdministrator => "Shelter site operations, bed allocations, and evacuee tracking",
                UserRoles.Volunteer => "Field engagement, operational execution, and frontline task completion",
                UserRoles.Citizen => "Public portal access, crisis self-reporting, and community shelter discovery",
                UserRoles.IncidentManager => "Crisis incident escalation, spatial dispatch, and field management",
                UserRoles.ShelterAdmin => "Shelter site operations, bed allocations, and evacuee tracking",
                UserRoles.VolunteerCoordinator => "Volunteer roster scheduling, qualification checks, and team assembly",
                UserRoles.LogisticsManager => "Supply chain logistics, aid stockpile oversight, and delivery dispatch",
                UserRoles.Donor => "Philanthropic fund allocation and aid donation monitoring",
                _ => "Custom platform role"
            };

            dtos.Add(new RoleDto
            {
                Id = role.Id,
                Name = roleName,
                Purpose = purpose,
                UsersCount = uCount,
                PermissionsCount = pCount,
                IsSystemRole = isSystem
            });
        }

        return ApiResponse<IReadOnlyList<RoleDto>>.SuccessResult(
            dtos.OrderByDescending(r => r.IsSystemRole).ThenBy(r => r.Name).ToList(),
            "Platform roles retrieved successfully.");
    }

    public async Task<ApiResponse<RoleDetailsDto>> GetRoleByIdAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id)
            ?? await _roleManager.FindByNameAsync(id);

        if (role == null)
        {
            return ApiResponse<RoleDetailsDto>.FailureResult("Role not found.");
        }

        var permissions = await GetPermissionsForRoleInternalAsync(role);
        var userCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
        var isSystem = UserRoles.AllRoles.Contains(role.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase);

        return ApiResponse<RoleDetailsDto>.SuccessResult(new RoleDetailsDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Purpose = isSystem ? $"System-defined role: {role.Name}" : "Custom role",
            UsersCount = userCount,
            IsSystemRole = isSystem,
            Permissions = permissions.ToList()
        }, "Role details retrieved successfully.");
    }

    public async Task<ApiResponse<RoleDetailsDto>> CreateRoleAsync(CreateRoleRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiResponse<RoleDetailsDto>.FailureResult("Role name cannot be empty.");
        }

        var normalizedName = request.Name.Trim();
        if (await _roleManager.RoleExistsAsync(normalizedName))
        {
            return ApiResponse<RoleDetailsDto>.FailureResult($"Role '{normalizedName}' already exists.");
        }

        var role = new IdentityRole(normalizedName);
        var createResult = await _roleManager.CreateAsync(role);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToList();
            return ApiResponse<RoleDetailsDto>.FailureResult("Failed to create role.", errors);
        }

        // Add valid permissions as claims
        var validPermissions = request.Permissions
            .Where(Permissions.IsValid)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var perm in validPermissions)
        {
            await _roleManager.AddClaimAsync(role, new Claim("Permission", perm));
        }

        _permissionService.InvalidateCache(roleId: role.Id);
        _logger.LogInformation("Custom role '{RoleName}' created with {PermCount} permissions.", role.Name, validPermissions.Count);

        return ApiResponse<RoleDetailsDto>.SuccessResult(new RoleDetailsDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Purpose = request.Purpose ?? "Custom role",
            UsersCount = 0,
            IsSystemRole = false,
            Permissions = validPermissions
        }, "Role created successfully.");
    }

    public async Task<ApiResponse<RoleDetailsDto>> UpdateRoleAsync(string id, UpdateRoleRequestDto request)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return ApiResponse<RoleDetailsDto>.FailureResult("Role not found.");
        }

        var isSystem = UserRoles.AllRoles.Contains(role.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        if (isSystem && !string.Equals(role.Name, request.Name, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<RoleDetailsDto>.FailureResult("System roles cannot be renamed.");
        }

        if (!isSystem && !string.IsNullOrWhiteSpace(request.Name) && !string.Equals(role.Name, request.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (await _roleManager.RoleExistsAsync(request.Name.Trim()))
            {
                return ApiResponse<RoleDetailsDto>.FailureResult($"Role '{request.Name}' already exists.");
            }
            role.Name = request.Name.Trim();
            var updateResult = await _roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
            {
                return ApiResponse<RoleDetailsDto>.FailureResult("Failed to update role name.");
            }
        }

        _permissionService.InvalidateCache(roleId: role.Id);
        return await GetRoleByIdAsync(role.Id);
    }

    public async Task<ApiResponse<bool>> DeleteRoleAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return ApiResponse<bool>.FailureResult("Role not found.");
        }

        if (UserRoles.AllRoles.Contains(role.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase))
        {
            return ApiResponse<bool>.FailureResult("System roles are protected and cannot be deleted.");
        }

        var assignedUsers = await _context.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
        if (assignedUsers > 0)
        {
            return ApiResponse<bool>.FailureResult($"Cannot delete role '{role.Name}' because it is assigned to {assignedUsers} user(s).");
        }

        // Delete associated claims first
        var claims = await _roleManager.GetClaimsAsync(role);
        foreach (var claim in claims)
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            return ApiResponse<bool>.FailureResult("Failed to delete role.");
        }

        _permissionService.InvalidateCache(roleId: role.Id);
        _logger.LogInformation("Role '{RoleName}' deleted successfully.", role.Name);
        return ApiResponse<bool>.SuccessResult(true, "Role deleted successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<string>>> GetRolePermissionsAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId)
            ?? await _roleManager.FindByNameAsync(roleId);

        if (role == null)
        {
            return ApiResponse<IReadOnlyList<string>>.FailureResult("Role not found.");
        }

        var permissions = await GetPermissionsForRoleInternalAsync(role);
        return ApiResponse<IReadOnlyList<string>>.SuccessResult(
            permissions, $"Permissions for role '{role.Name}' retrieved successfully.");
    }

    public async Task<ApiResponse<bool>> UpdateRolePermissionsAsync(string roleId, UpdateRolePermissionsRequestDto request)
    {
        var role = await _roleManager.FindByIdAsync(roleId)
            ?? await _roleManager.FindByNameAsync(roleId);

        if (role == null)
        {
            return ApiResponse<bool>.FailureResult("Role not found.");
        }

        var roleName = role.Name ?? string.Empty;

        // Prevent locking out Administrators
        if (UserRoles.IsAdminRole(roleName))
        {
            if (!request.Permissions.Contains(Permissions.Users.ManagePermissions) ||
                !request.Permissions.Contains(Permissions.Users.AssignRoles))
            {
                return ApiResponse<bool>.FailureResult(
                    "Administrator roles must retain administrative management permissions to prevent accidental system lockout.");
            }
        }

        var validPermissions = request.Permissions
            .Where(Permissions.IsValid)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Remove current permission claims
        var existingClaims = await _roleManager.GetClaimsAsync(role);
        foreach (var c in existingClaims.Where(c => c.Type == "Permission"))
        {
            await _roleManager.RemoveClaimAsync(role, c);
        }

        // Add updated claims
        foreach (var p in validPermissions)
        {
            await _roleManager.AddClaimAsync(role, new Claim("Permission", p));
        }

        _permissionService.InvalidateCache(roleId: role.Id);
        _logger.LogInformation("Role '{RoleName}' permissions updated to {Count} items.", roleName, validPermissions.Count);

        return ApiResponse<bool>.SuccessResult(true, $"Permissions for role '{roleName}' updated successfully.");
    }

    private async Task<IReadOnlyList<string>> GetPermissionsForRoleInternalAsync(IdentityRole role)
    {
        var roleName = role.Name ?? string.Empty;
        if (UserRoles.IsAdminRole(roleName))
        {
            return Permissions.All;
        }

        var claims = await _roleManager.GetClaimsAsync(role);
        var permClaims = claims
            .Where(c => c.Type == "Permission" && !string.IsNullOrWhiteSpace(c.Value))
            .Select(c => c.Value)
            .ToList();

        if (permClaims.Count > 0)
        {
            return permClaims.OrderBy(p => p).ToList();
        }

        // Fallback to seeded defaults
        return DefaultRolePermissions.GetDefaultPermissions(roleName).OrderBy(p => p).ToList();
    }
}
