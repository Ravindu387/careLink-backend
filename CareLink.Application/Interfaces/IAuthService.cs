using CareLink.Application.Common;
using CareLink.Application.DTOs.Auth;

namespace CareLink.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponse<UserDto>> GetCurrentUserAsync(string userId);
}
