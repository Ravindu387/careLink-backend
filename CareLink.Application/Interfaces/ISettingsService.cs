using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.Settings;

namespace CareLink.Application.Interfaces;

public interface ISettingsService
{
    // Platform System Settings (Admin / System Admin)
    Task<ApiResponse<SystemSettingsDto>> GetSettingsAsync();
    Task<ApiResponse<SystemSettingsDto>> UpdateSettingsAsync(UpdateSystemSettingsDto request, string actor);
    Task<ApiResponse<SystemSettingsDto>> ResetDefaultsAsync(string actor);

    // Emergency Coordinator Settings
    Task<CoordinatorSettingsDto> GetCoordinatorSettingsAsync(CancellationToken cancellationToken = default);
    Task<CoordinatorProfileSettingsDto> UpdateCoordinatorProfileAsync(UpdateCoordinatorProfileSettingsDto dto, CancellationToken cancellationToken = default);
    Task<CoordinatorOperationalSettingsDto> UpdateCoordinatorOperationalAsync(UpdateCoordinatorOperationalSettingsDto dto, CancellationToken cancellationToken = default);
    Task<CoordinatorNotificationSettingsDto> UpdateCoordinatorNotificationsAsync(UpdateCoordinatorNotificationSettingsDto dto, CancellationToken cancellationToken = default);
    Task<ChangePasswordResponseDto> ChangeCoordinatorPasswordAsync(ChangePasswordRequestDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSessionDto>> GetCoordinatorSessionsAsync(CancellationToken cancellationToken = default);
    Task<bool> RevokeSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<CoordinatorSettingsDto> ResetCoordinatorSettingsAsync(CancellationToken cancellationToken = default);

    // Partner Officer Settings
    Task<PartnerSettingsDto> GetPartnerSettingsAsync(CancellationToken cancellationToken = default);
    Task<PartnerProfileSettingsDto> UpdatePartnerProfileAsync(UpdatePartnerProfileSettingsDto dto, CancellationToken cancellationToken = default);
    Task<PartnerOrganisationSettingsDto> UpdatePartnerOrganisationAsync(UpdatePartnerOrganisationSettingsDto dto, CancellationToken cancellationToken = default);
    Task<PartnerSharingSettingsDto> UpdatePartnerSharingAsync(UpdatePartnerSharingSettingsDto dto, CancellationToken cancellationToken = default);
    Task<PartnerNotificationSettingsDto> UpdatePartnerNotificationsAsync(UpdatePartnerNotificationSettingsDto dto, CancellationToken cancellationToken = default);
    Task<PartnerSecuritySettingsDto> UpdatePartnerSecurityAsync(UpdatePartnerSecuritySettingsDto dto, CancellationToken cancellationToken = default);
    Task<ChangePasswordResponseDto> ChangePartnerPasswordAsync(ChangePasswordRequestDto dto, CancellationToken cancellationToken = default);
    Task<PartnerSettingsDto> ResetPartnerSettingsAsync(CancellationToken cancellationToken = default);

    // Shelter Admin Settings
    Task<ProfileDto> GetProfileAsync();
    Task<ProfileDto> UpdateProfileAsync(ProfileDto dto);
    Task<ShelterSettingsDto> GetShelterSettingsAsync();
    Task<ShelterSettingsDto> UpdateShelterSettingsAsync(ShelterSettingsDto dto);
    Task<CareLink.Application.DTOs.Settings.SecuritySettingsDto> GetSecuritySettingsAsync();
    Task<bool> UpdateSecuritySettingsAsync(CareLink.Application.DTOs.Settings.SecuritySettingsDto dto);
    Task<NotificationSettingsDto> GetNotificationSettingsAsync();
    Task<NotificationSettingsDto> UpdateNotificationSettingsAsync(NotificationSettingsDto dto);

    // Resource Manager Settings (Oshadi)
    Task<ApiResponse<ResourceManagerSettingsDto>> GetResourceManagerSettingsAsync();
    Task<ApiResponse<ResourceManagerSettingsDto>> UpdateSettingsAsync(UpdateSettingsRequest request);
    Task<ApiResponse<ResourceManagerSettingsDto>> ResetSettingsAsync();
    Task<ApiResponse<ProfileSettingsDto>> UpdateProfileAsync(ProfileSettingsDto dto);
    Task<ApiResponse<InventoryRulesSettingsDto>> UpdateInventoryRulesAsync(InventoryRulesSettingsDto dto);
    Task<ApiResponse<DispatchPreferencesSettingsDto>> UpdateDispatchPreferencesAsync(DispatchPreferencesSettingsDto dto);
    Task<ApiResponse<NotificationPreferencesSettingsDto>> UpdateNotificationPreferencesAsync(NotificationPreferencesSettingsDto dto);
    Task<ApiResponse<CareLink.Application.DTOs.SecuritySettingsDto>> UpdateResourceManagerSecuritySettingsAsync(CareLink.Application.DTOs.SecuritySettingsDto dto);
}
