using CareLink.Application.Common;
using CareLink.Application.DTOs.Roles;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IPermissionService _permissionService;

    private static readonly HashSet<string> PrivilegedRoleNames = new(StringComparer.OrdinalIgnoreCase)
    {
        UserRoles.Admin,
        UserRoles.SystemAdmin,
        UserRoles.EmergencyCoordinator,
        UserRoles.IncidentManager,
        UserRoles.LogisticsManager,
        UserRoles.ShelterAdmin,
        UserRoles.PartnerOfficer
    };

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IPermissionService permissionService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _permissionService = permissionService;
    }

    public async Task<ApiResponse<PaginatedListDto<UserDetailsDto>>> GetUsersAsync(UserListQueryDto query)
    {
        var usersQuery = _userManager.Users
            .Include(u => u.Organization)
            .AsNoTracking()
            .AsQueryable();

        // 1. Text Search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            usersQuery = usersQuery.Where(u =>
                u.FullName.ToLower().Contains(search) ||
                (u.Email != null && u.Email.ToLower().Contains(search)) ||
                (u.NIC != null && u.NIC.ToLower().Contains(search)) ||
                (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(search)) ||
                (u.Organization != null && u.Organization.Name.ToLower().Contains(search)));
        }

        // 2. Status Filter
        if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            if (query.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                usersQuery = usersQuery.Where(u => u.IsActive);
            }
            else if (query.Status.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
            {
                usersQuery = usersQuery.Where(u => !u.IsActive);
            }
            else if (query.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                usersQuery = usersQuery.Where(u => !u.EmailConfirmed);
            }
        }

        // 3. Organization Filter
        if (!string.IsNullOrWhiteSpace(query.Organization) && !query.Organization.Equals("All organisations", StringComparison.OrdinalIgnoreCase))
        {
            usersQuery = usersQuery.Where(u => u.Organization != null && u.Organization.Name == query.Organization);
        }

        // 4. Role Filter
        if (!string.IsNullOrWhiteSpace(query.Role) && !query.Role.Equals("All roles", StringComparison.OrdinalIgnoreCase))
        {
            var normalizedRole = NormalizeRoleName(query.Role);
            var usersInRole = await _userManager.GetUsersInRoleAsync(normalizedRole);
            var userIdsInRole = usersInRole.Select(u => u.Id).ToHashSet();
            usersQuery = usersQuery.Where(u => userIdsInRole.Contains(u.Id));
        }

        // 5. Tab Filter
        if (!string.IsNullOrWhiteSpace(query.Tab) && !query.Tab.Equals("All users", StringComparison.OrdinalIgnoreCase))
        {
            if (query.Tab.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
            {
                usersQuery = usersQuery.Where(u => !u.IsActive);
            }
            else if (query.Tab.Equals("Pending approval", StringComparison.OrdinalIgnoreCase))
            {
                usersQuery = usersQuery.Where(u => !u.EmailConfirmed);
            }
            else if (query.Tab.Equals("Privileged users", StringComparison.OrdinalIgnoreCase))
            {
                var privilegedUserIds = new HashSet<string>();
                foreach (var role in PrivilegedRoleNames)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                    foreach (var u in usersInRole)
                    {
                        privilegedUserIds.Add(u.Id);
                    }
                }

                usersQuery = usersQuery.Where(u => privilegedUserIds.Contains(u.Id));
            }
        }

        var totalCount = await usersQuery.CountAsync();

        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 50;

        var pagedUsers = await usersQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<UserDetailsDto>();
        foreach (var user in pagedUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(MapToUserDetailsDto(user, roles));
        }

        var result = new PaginatedListDto<UserDetailsDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PaginatedListDto<UserDetailsDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<UserDetailsDto>> GetUserByIdAsync(string id)
    {
        var user = await _userManager.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return ApiResponse<UserDetailsDto>.FailureResult("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = (await _permissionService.GetUserEffectivePermissionsAsync(user.Id)).ToList();
        var dto = MapToUserDetailsDto(user, roles, permissions);

        return ApiResponse<UserDetailsDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<UserStatsDto>> GetUserStatsAsync()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var activeUsers = await _userManager.Users.CountAsync(u => u.IsActive);
        var disabledUsers = await _userManager.Users.CountAsync(u => !u.IsActive);
        var pendingUsers = await _userManager.Users.CountAsync(u => !u.EmailConfirmed);

        var privilegedUserIds = new HashSet<string>();
        foreach (var role in PrivilegedRoleNames)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            foreach (var u in usersInRole)
            {
                privilegedUserIds.Add(u.Id);
            }
        }

        var stats = new UserStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            DisabledUsers = disabledUsers,
            PendingUsers = pendingUsers,
            PrivilegedUsers = privilegedUserIds.Count
        };

        return ApiResponse<UserStatsDto>.SuccessResult(stats);
    }

    public async Task<ApiResponse<UserDetailsDto>> CreateUserAsync(CreateUserRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ApiResponse<UserDetailsDto>.FailureResult("An account with this email address already exists.");
        }

        var assignedRole = NormalizeRoleName(request.Role);
        if (!UserRoles.IsValidRole(assignedRole))
        {
            return ApiResponse<UserDetailsDto>.FailureResult(
                $"Invalid role: '{request.Role}'. Valid roles: {string.Join(", ", UserRoles.AllRoles)}");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            NIC = request.NIC?.Trim(),
            Address = request.Address?.Trim(),
            OrganizationId = request.OrganizationId,
            IsActive = request.IsActive,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var password = string.IsNullOrWhiteSpace(request.Password) ? "CareLink@2026" : request.Password;
        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToList();
            return ApiResponse<UserDetailsDto>.FailureResult("Failed to create user.", errors);
        }

        if (!await _roleManager.RoleExistsAsync(assignedRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(assignedRole));
        }

        await _userManager.AddToRoleAsync(user, assignedRole);

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = (await _permissionService.GetUserEffectivePermissionsAsync(user.Id)).ToList();
        var dto = MapToUserDetailsDto(user, roles, permissions);

        return ApiResponse<UserDetailsDto>.SuccessResult(dto, "User created successfully.");
    }

    public async Task<ApiResponse<UserDetailsDto>> UpdateUserAsync(string id, UpdateUserRequestDto request)
    {
        var user = await _userManager.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return ApiResponse<UserDetailsDto>.FailureResult("User not found.");
        }

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.NIC = request.NIC?.Trim();
        user.Address = request.Address?.Trim();
        user.OrganizationId = request.OrganizationId;
        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.Select(e => e.Description).ToList();
            return ApiResponse<UserDetailsDto>.FailureResult("Failed to update user.", errors);
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var newRole = NormalizeRoleName(request.Role);
            if (UserRoles.IsValidRole(newRole))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (!currentRoles.Contains(newRole))
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!await _roleManager.RoleExistsAsync(newRole))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(newRole));
                    }
                    await _userManager.AddToRoleAsync(user, newRole);
                }
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = (await _permissionService.GetUserEffectivePermissionsAsync(user.Id)).ToList();
        var dto = MapToUserDetailsDto(user, roles, permissions);

        return ApiResponse<UserDetailsDto>.SuccessResult(dto, "User updated successfully.");
    }

    public async Task<ApiResponse<bool>> UpdateUserStatusAsync(string id, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return ApiResponse<bool>.FailureResult("User not found.");
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ApiResponse<bool>.FailureResult("Failed to update user status.");
        }

        var statusText = isActive ? "activated" : "deactivated";
        return ApiResponse<bool>.SuccessResult(true, $"User account {statusText} successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return ApiResponse<bool>.FailureResult("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains(UserRoles.SystemAdmin))
        {
            var systemAdmins = await _userManager.GetUsersInRoleAsync(UserRoles.SystemAdmin);
            if (systemAdmins.Count <= 1)
            {
                return ApiResponse<bool>.FailureResult("Cannot delete the last System Administrator account.");
            }
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return ApiResponse<bool>.FailureResult("Failed to delete user.");
        }

        _permissionService.InvalidateCache(userId: user.Id);
        return ApiResponse<bool>.SuccessResult(true, "User deleted successfully.");
    }

    public async Task<ApiResponse<bool>> UpdateUserRolesAsync(string userId, UpdateUserRolesRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<bool>.FailureResult("User not found.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        // Normalize and validate requested roles
        var validRoles = request.Roles
            .Select(NormalizeRoleName)
            .Where(UserRoles.IsValidRole)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (validRoles.Count == 0)
        {
            validRoles.Add(UserRoles.Citizen);
        }

        // Safeguard: Check if removing Admin/SystemAdmin leaves 0 active administrators
        var isCurrentlyAdmin = currentRoles.Any(UserRoles.IsAdminRole);
        var willBeAdmin = validRoles.Any(UserRoles.IsAdminRole);

        if (isCurrentlyAdmin && !willBeAdmin)
        {
            var adminUsers = (await _userManager.GetUsersInRoleAsync(UserRoles.Admin)).ToList();
            var systemAdminUsers = (await _userManager.GetUsersInRoleAsync(UserRoles.SystemAdmin)).ToList();
            var remainingAdmins = adminUsers.Concat(systemAdminUsers)
                .Where(u => u.IsActive && u.Id != user.Id)
                .Select(u => u.Id)
                .Distinct()
                .Count();

            if (remainingAdmins == 0)
            {
                return ApiResponse<bool>.FailureResult("Cannot remove the last active administrator from the system.");
            }
        }

        // Ensure roles exist in database
        foreach (var role in validRoles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            return ApiResponse<bool>.FailureResult("Failed to clear current user roles.");
        }

        var addResult = await _userManager.AddToRolesAsync(user, validRoles);
        if (!addResult.Succeeded)
        {
            return ApiResponse<bool>.FailureResult("Failed to assign new user roles.");
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _permissionService.InvalidateCache(userId: user.Id);

        return ApiResponse<bool>.SuccessResult(true, $"Assigned roles for user '{user.FullName}' updated to: {string.Join(", ", validRoles)}.");
    }

    public async Task<ApiResponse<IReadOnlyList<string>>> GetUserPermissionsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<IReadOnlyList<string>>.FailureResult("User not found.");
        }

        var permissions = await _permissionService.GetUserEffectivePermissionsAsync(userId);
        return ApiResponse<IReadOnlyList<string>>.SuccessResult(permissions, "Effective user permissions retrieved successfully.");
    }

    private static UserDetailsDto MapToUserDetailsDto(ApplicationUser user, IList<string> roles, IList<string>? permissions = null)
    {
        return new UserDetailsDto
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
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.UpdatedAt,
            LastActive = FormatLastActive(user.UpdatedAt ?? user.CreatedAt)
        };
    }

    private static string FormatLastActive(DateTime? dateTime)
    {
        if (!dateTime.HasValue) return "Never";
        var span = DateTime.UtcNow - dateTime.Value;
        if (span.TotalMinutes < 2) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} hr ago";
        if (span.TotalDays < 2) return "Yesterday";
        if (span.TotalDays < 30) return $"{(int)span.TotalDays} days ago";
        return dateTime.Value.ToString("MMM dd, yyyy");
    }

    private static string NormalizeRoleName(string role)
    {
        if (string.IsNullOrWhiteSpace(role)) return UserRoles.Citizen;
        var trimmed = role.Trim();

        return trimmed.ToLowerInvariant() switch
        {
            "admin" or "administrator" => UserRoles.Admin,
            "emergency coordinator" or "emergencycoordinator" => UserRoles.EmergencyCoordinator,
            "incident manager" or "incidentmanager" => UserRoles.IncidentManager,
            "logistics manager" or "logisticsmanager" or "resource manager" or "resourcemanager" => UserRoles.LogisticsManager,
            "shelter admin" or "shelteradmin" or "shelter administrator" or "shelteradministrator" => UserRoles.ShelterAdmin,
            "volunteer coordinator" or "volunteercoordinator" => UserRoles.VolunteerCoordinator,
            "donor" => UserRoles.Donor,
            "partner officer" or "partner organisation officer" or "partner organization officer" or "partnerofficer" => UserRoles.PartnerOfficer,
            "system admin" or "systemadmin" or "system administrator" or "systemadministrator" => UserRoles.SystemAdmin,
            "volunteer" => UserRoles.Volunteer,
            "citizen" => UserRoles.Citizen,
            _ => UserRoles.IsValidRole(trimmed) ? trimmed : UserRoles.Citizen
        };
    }
}
