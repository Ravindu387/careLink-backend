using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Permissions;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using CareLink.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CareLink.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PermissionService> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public PermissionService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IMemoryCache cache,
        ILogger<PermissionService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _cache = cache;
        _logger = logger;
    }

    public Task<ApiResponse<IReadOnlyList<PermissionCategoryDto>>> GetAllPermissionsAsync()
    {
        var categorized = GetGroupedPermissions();
        return Task.FromResult(ApiResponse<IReadOnlyList<PermissionCategoryDto>>.SuccessResult(
            categorized, "System permissions retrieved successfully."));
    }

    public async Task<IReadOnlyList<string>> GetUserEffectivePermissionsAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Array.Empty<string>();

        var cacheKey = $"user_effective_permissions_{userId}";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<string>? cached) && cached != null)
        {
            return cached;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null ||
            !user.IsActive ||
            user.RequiresPasswordSetup ||
            !string.Equals(user.AccountStatus, AccountStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            return Array.Empty<string>();
        }

        var roles = await _userManager.GetRolesAsync(user);

        // System administrators and Admins have full access to all permissions
        if (roles.Any(UserRoles.IsAdminRole))
        {
            var all = Permissions.All;
            _cache.Set(cacheKey, all, CacheDuration);
            return all;
        }

        var permissions = await GetRolesEffectivePermissionsAsync(roles);
        _cache.Set(cacheKey, permissions, CacheDuration);
        return permissions;
    }

    public async Task<IReadOnlyList<string>> GetRolesEffectivePermissionsAsync(IEnumerable<string> roleNames)
    {
        var roleList = roleNames.ToList();
        if (roleList.Count == 0)
            return Array.Empty<string>();

        if (roleList.Any(UserRoles.IsAdminRole))
        {
            return Permissions.All;
        }

        var distinctPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var roles = await _context.Roles
            .Where(r => roleList.Contains(r.Name!))
            .ToListAsync();

        var roleIds = roles.Select(r => r.Id).ToList();

        var dbClaims = await _context.RoleClaims
            .Where(rc => roleIds.Contains(rc.RoleId) && rc.ClaimType == "Permission" && rc.ClaimValue != null)
            .Select(rc => rc.ClaimValue!)
            .ToListAsync();

        foreach (var claim in dbClaims)
        {
            distinctPermissions.Add(claim);
        }

        // Fallback to default role permissions if database has not recorded explicit claims for standard roles
        foreach (var role in roleList)
        {
            var defaults = DefaultRolePermissions.GetDefaultPermissions(role);
            foreach (var def in defaults)
            {
                distinctPermissions.Add(def);
            }
        }

        return distinctPermissions.OrderBy(p => p).ToList();
    }

    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(permission))
            return false;

        var userPermissions = await GetUserEffectivePermissionsAsync(userId);
        return userPermissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public void InvalidateCache(string? userId = null, string? roleId = null)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            _cache.Remove($"user_effective_permissions_{userId}");
        }

        _cache.Remove("all_roles_with_permissions");
    }

    private static IReadOnlyList<PermissionCategoryDto> GetGroupedPermissions()
    {
        var dict = new Dictionary<string, List<PermissionDto>>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in Permissions.All)
        {
            var parts = p.Split('.');
            var category = parts[0];
            var action = parts.Length > 1 ? parts[1] : p;

            var categoryName = category switch
            {
                "Users" => "User Management",
                "Crises" => "Crisis Management",
                "Requests" => "Assistance Requests",
                "AIRecommendations" => "AI Recommendations",
                "Resources" => "Resource Management",
                "Shelters" => "Shelter Management",
                "Volunteers" => "Volunteer Management",
                "Donations" => "Donation Management",
                "Reports" => "Reports",
                "Audit" => "Audit & Compliance",
                _ => category
            };

            var displayName = action switch
            {
                "View" => $"View {category}",
                "Create" => $"Create {category}",
                "Update" => $"Edit {category}",
                "Delete" => $"Delete {category}",
                "AssignRoles" => "Assign User Roles",
                "ManagePermissions" => "Manage Permissions & Roles",
                "Manage" => $"Manage {category}",
                "Verify" => $"Verify {category}",
                "Assign" => $"Assign {category}",
                "Resolve" => $"Resolve {category}",
                "Approve" => $"Approve {category}",
                "Modify" => $"Modify {category}",
                "Defer" => $"Defer {category}",
                "Allocate" => $"Allocate {category}",
                "ManageOccupancy" => "Manage Shelter Occupancy",
                "Export" => $"Export {category}",
                _ => $"{action} {category}"
            };

            var description = $"Allows user to {action.ToLower()} {category.ToLower()} operational resources.";

            if (!dict.TryGetValue(categoryName, out var list))
            {
                list = new List<PermissionDto>();
                dict[categoryName] = list;
            }

            list.Add(new PermissionDto
            {
                Name = p,
                DisplayName = displayName,
                Category = categoryName,
                Description = description
            });
        }

        return dict.Select(kvp => new PermissionCategoryDto
        {
            Category = kvp.Key,
            Permissions = kvp.Value
        }).ToList();
    }
}
