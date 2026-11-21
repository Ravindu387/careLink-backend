namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface ICitizenProfileService
{
    Task<ApiResponse<CitizenProfileSettingsDto>> GetProfileAsync(string? citizenName = null);
    Task<ApiResponse<CitizenProfileSettingsDto>> UpdateFullProfileAsync(UpdateFullProfileRequest request, string? citizenName = null);
    Task<ApiResponse<CitizenProfileSettingsDto>> UpdatePersonalDetailsAsync(UpdatePersonalDetailsRequest request, string? citizenName = null);
    Task<ApiResponse<CitizenProfileSettingsDto>> UpdateContactLocationAsync(UpdateContactLocationRequest request, string? citizenName = null);
    Task<ApiResponse<CitizenProfileSettingsDto>> UpdateLanguageAccessibilityAsync(UpdateLanguageAccessibilityRequest request, string? citizenName = null);
    Task<ApiResponse<CitizenProfileSettingsDto>> UpdateNotificationPreferencesAsync(UpdateNotificationPreferencesRequest request, string? citizenName = null);
    Task<ApiResponse<CitizenProfileSettingsDto>> UpdatePrivacyDataAsync(UpdatePrivacyDataRequest request, string? citizenName = null);
    Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request, string? citizenName = null);
    Task<ApiResponse<CitizenProfileSettingsDto>> ToggleTwoFactorAsync(bool enabled, string? citizenName = null);
    Task<ApiResponse<CitizenProfileSettingsDto>> ResetProfileAsync(string? citizenName = null);
}
