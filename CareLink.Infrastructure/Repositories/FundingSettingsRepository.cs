using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class FundingSettingsRepository : IFundingSettingsRepository
{
    private readonly CareLinkDbContext _context;

    public FundingSettingsRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<FundingSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _context.FundingSettings.FirstOrDefaultAsync(s => s.Id == "funding-officer-default", cancellationToken);
        if (settings == null)
        {
            settings = new FundingSettings
            {
                Id = "funding-officer-default",
                FullName = "Maria Kone",
                Email = "maria@reliefos.org",
                Contact = "+94 77 412 8041",
                Currency = "USD",
                ApprovalLimit = 50000m,
                RequireApproval = true,
                DualApproval = true,
                AutoVerification = false,
                RestrictedAllocation = true,
                EmailNotifications = true,
                InAppNotifications = true,
                SmsNotifications = false,
                NotifyDonations = true,
                NotifyApprovals = true,
                NotifyCampaigns = true,
                NotifySecurity = true,
                TwoFactorEnabled = true,
                ActiveDevice = "Windows · Chrome",
                LastPasswordUpdate = "Recently",
                UpdatedAt = DateTime.UtcNow
            };

            await _context.FundingSettings.AddAsync(settings, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return settings;
    }

    public async Task<FundingSettings> UpdateSettingsAsync(FundingSettings settings, CancellationToken cancellationToken = default)
    {
        var existing = await GetSettingsAsync(cancellationToken);

        existing.FullName = settings.FullName.Trim();
        existing.Email = settings.Email.Trim();
        existing.Contact = settings.Contact.Trim();
        existing.Currency = settings.Currency.Trim();

        existing.ApprovalLimit = settings.ApprovalLimit;
        existing.RequireApproval = settings.RequireApproval;
        existing.DualApproval = settings.DualApproval;
        existing.AutoVerification = settings.AutoVerification;
        existing.RestrictedAllocation = settings.RestrictedAllocation;

        existing.EmailNotifications = settings.EmailNotifications;
        existing.InAppNotifications = settings.InAppNotifications;
        existing.SmsNotifications = settings.SmsNotifications;

        existing.NotifyDonations = settings.NotifyDonations;
        existing.NotifyApprovals = settings.NotifyApprovals;
        existing.NotifyCampaigns = settings.NotifyCampaigns;
        existing.NotifySecurity = settings.NotifySecurity;

        existing.TwoFactorEnabled = settings.TwoFactorEnabled;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.FundingSettings.Update(existing);
        await _context.SaveChangesAsync(cancellationToken);

        return existing;
    }

    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var existing = await GetSettingsAsync(cancellationToken);
        existing.LastPasswordUpdate = "Just now";
        existing.UpdatedAt = DateTime.UtcNow;

        _context.FundingSettings.Update(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
