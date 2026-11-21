namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface ISettingsService
{
    Task<ApiResponse<ResourceManagerSettingsDto>> GetSettingsAsync();
    Task<ApiResponse<ResourceManagerSettingsDto>> UpdateSettingsAsync(UpdateSettingsRequest request);
    Task<ApiResponse<ResourceManagerSettingsDto>> ResetSettingsAsync();
    Task<ApiResponse<ProfileSettingsDto>> UpdateProfileAsync(ProfileSettingsDto dto);
    Task<ApiResponse<InventoryRulesSettingsDto>> UpdateInventoryRulesAsync(InventoryRulesSettingsDto dto);
    Task<ApiResponse<DispatchPreferencesSettingsDto>> UpdateDispatchPreferencesAsync(DispatchPreferencesSettingsDto dto);
    Task<ApiResponse<NotificationPreferencesSettingsDto>> UpdateNotificationPreferencesAsync(NotificationPreferencesSettingsDto dto);
    Task<ApiResponse<SecuritySettingsDto>> UpdateSecuritySettingsAsync(SecuritySettingsDto dto);
}
