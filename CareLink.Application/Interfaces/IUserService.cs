using CareLink.Application.Common;
using CareLink.Application.DTOs.Users;

namespace CareLink.Application.Interfaces;

public interface IUserService
{
    Task<ApiResponse<PaginatedListDto<UserDetailsDto>>> GetUsersAsync(UserListQueryDto query);
    Task<ApiResponse<UserDetailsDto>> GetUserByIdAsync(string id);
    Task<ApiResponse<UserStatsDto>> GetUserStatsAsync();
    Task<ApiResponse<UserDetailsDto>> CreateUserAsync(CreateUserRequestDto request);
    Task<ApiResponse<UserDetailsDto>> UpdateUserAsync(string id, UpdateUserRequestDto request);
    Task<ApiResponse<bool>> UpdateUserStatusAsync(string id, bool isActive);
    Task<ApiResponse<bool>> DeleteUserAsync(string id);
}
