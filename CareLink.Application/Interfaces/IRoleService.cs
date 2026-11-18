using CareLink.Application.Common;
using CareLink.Application.DTOs.Roles;

namespace CareLink.Application.Interfaces;

public interface IRoleService
{
    Task<ApiResponse<IReadOnlyList<RoleDto>>> GetRolesAsync();
    Task<ApiResponse<RoleDetailsDto>> GetRoleByIdAsync(string id);
    Task<ApiResponse<RoleDetailsDto>> CreateRoleAsync(CreateRoleRequestDto request);
    Task<ApiResponse<RoleDetailsDto>> UpdateRoleAsync(string id, UpdateRoleRequestDto request);
    Task<ApiResponse<bool>> DeleteRoleAsync(string id);
    Task<ApiResponse<IReadOnlyList<string>>> GetRolePermissionsAsync(string roleId);
    Task<ApiResponse<bool>> UpdateRolePermissionsAsync(string roleId, UpdateRolePermissionsRequestDto request);
}
