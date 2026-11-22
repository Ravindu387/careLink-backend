using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface ICaseManagerSettingsService
{
    Task<CaseManagerSettingsDto> GetSettingsAsync();
    Task<CaseManagerSettingsDto> UpdateSettingsAsync(CaseManagerSettingsDto settings);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    Task<bool> RevokeSessionAsync(string sessionId);
    Task<CaseManagerSettingsDto> ResetDefaultsAsync();
}
