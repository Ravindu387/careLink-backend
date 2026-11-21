using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IVolunteerSettingsService
{
    Task<VolunteerSettingsDto> GetSettingsAsync();
    Task<VolunteerSettingsDto> UpdateSettingsAsync(VolunteerSettingsDto settings);
    Task<bool> ChangePasswordAsync(VolunteerChangePasswordRequest request);
    Task<bool> RevokeSessionAsync(string sessionId);
    Task<VolunteerSettingsDto> ResetDefaultsAsync();
}
