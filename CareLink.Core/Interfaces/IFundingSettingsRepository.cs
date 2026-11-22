using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public interface IFundingSettingsRepository
{
    Task<FundingSettings> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<FundingSettings> UpdateSettingsAsync(FundingSettings settings, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}
