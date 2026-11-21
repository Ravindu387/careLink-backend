using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IFundingSettingsService
{
    Task<FundingSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<FundingSettingsDto> UpdateSettingsAsync(FundingSettingsDto dto, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(ChangeFundingPasswordDto dto, CancellationToken cancellationToken = default);
}
