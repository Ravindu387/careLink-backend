using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface ISettingsService
{
    Task<ProfileDto> GetProfileAsync();
    Task<ProfileDto> UpdateProfileAsync(ProfileDto dto);
    Task<ShelterSettingsDto> GetShelterSettingsAsync();
    Task<ShelterSettingsDto> UpdateShelterSettingsAsync(ShelterSettingsDto dto);
    Task<SecuritySettingsDto> GetSecuritySettingsAsync();
    Task<bool> UpdateSecuritySettingsAsync(SecuritySettingsDto dto);
    Task<NotificationSettingsDto> GetNotificationSettingsAsync();
    Task<NotificationSettingsDto> UpdateNotificationSettingsAsync(NotificationSettingsDto dto);
}
