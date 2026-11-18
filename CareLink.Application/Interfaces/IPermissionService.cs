using CareLink.Application.Common;
using CareLink.Application.DTOs.Permissions;

namespace CareLink.Application.Interfaces;

public interface IPermissionService
{
    Task<ApiResponse<IReadOnlyList<PermissionCategoryDto>>> GetAllPermissionsAsync();
    Task<IReadOnlyList<string>> GetUserEffectivePermissionsAsync(string userId);
    Task<IReadOnlyList<string>> GetRolesEffectivePermissionsAsync(IEnumerable<string> roleNames);
    Task<bool> HasPermissionAsync(string userId, string permission);
    void InvalidateCache(string? userId = null, string? roleId = null);
}
