using CareLink.Application.Common;
using CareLink.Application.DTOs.Settings;

namespace CareLink.Application.Interfaces;

public interface ISettingsService
{
    Task<ApiResponse<SystemSettingsDto>> GetSettingsAsync();
    Task<ApiResponse<SystemSettingsDto>> UpdateSettingsAsync(UpdateSystemSettingsDto request, string actor);
    Task<ApiResponse<SystemSettingsDto>> ResetDefaultsAsync(string actor);
}
