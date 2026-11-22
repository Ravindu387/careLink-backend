using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class FundingSettingsService : IFundingSettingsService
{
    private readonly IFundingSettingsRepository _repository;

    public FundingSettingsService(IFundingSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<FundingSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetSettingsAsync(cancellationToken);
        return MapToDto(settings);
    }

    public async Task<FundingSettingsDto> UpdateSettingsAsync(FundingSettingsDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new FundingSettings
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Contact = dto.Contact,
            Currency = dto.Currency,
            ApprovalLimit = dto.ApprovalLimit,
            RequireApproval = dto.RequireApproval,
            DualApproval = dto.DualApproval,
            AutoVerification = dto.AutoVerification,
            RestrictedAllocation = dto.RestrictedAllocation,
            EmailNotifications = dto.EmailNotifications,
            InAppNotifications = dto.InAppNotifications,
            SmsNotifications = dto.SmsNotifications,
            NotifyDonations = dto.NotifyDonations,
            NotifyApprovals = dto.NotifyApprovals,
            NotifyCampaigns = dto.NotifyCampaigns,
            NotifySecurity = dto.NotifySecurity,
            TwoFactorEnabled = dto.TwoFactorEnabled
        };

        var updated = await _repository.UpdateSettingsAsync(entity, cancellationToken);
        return MapToDto(updated);
    }

    public async Task<bool> ChangePasswordAsync(ChangeFundingPasswordDto dto, CancellationToken cancellationToken = default)
    {
        return await _repository.ChangePasswordAsync(dto.CurrentPassword, dto.NewPassword, cancellationToken);
    }

    private static FundingSettingsDto MapToDto(FundingSettings s) => new()
    {
        FullName = s.FullName,
        Email = s.Email,
        Contact = s.Contact,
        Currency = s.Currency,
        ApprovalLimit = s.ApprovalLimit,
        RequireApproval = s.RequireApproval,
        DualApproval = s.DualApproval,
        AutoVerification = s.AutoVerification,
        RestrictedAllocation = s.RestrictedAllocation,
        EmailNotifications = s.EmailNotifications,
        InAppNotifications = s.InAppNotifications,
        SmsNotifications = s.SmsNotifications,
        NotifyDonations = s.NotifyDonations,
        NotifyApprovals = s.NotifyApprovals,
        NotifyCampaigns = s.NotifyCampaigns,
        NotifySecurity = s.NotifySecurity,
        TwoFactorEnabled = s.TwoFactorEnabled,
        ActiveDevice = s.ActiveDevice,
        LastPasswordUpdate = s.LastPasswordUpdate
    };
}
