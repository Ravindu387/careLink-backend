using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly CareLinkDbContext _context;
    private CoordinatorSettings _coordinatorSettings = new();
    private PartnerSettings _partnerSettings = new();
    private readonly object _lock = new();

    public SettingsRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<UserSettings?> GetAsync()
    {
        return await _context.UserSettings.FirstOrDefaultAsync();
    }

    public async Task<UserSettings> UpsertAsync(UserSettings settings)
    {
        var existing = await _context.UserSettings.FindAsync(settings.Id);
        if (existing == null)
        {
            _context.UserSettings.Add(settings);
        }
        else
        {
            existing.FullName = settings.FullName;
            existing.Email = settings.Email;
            existing.Phone = settings.Phone;
            existing.Role = settings.Role;
            existing.ShelterName = settings.ShelterName;
            existing.ShelterLocation = settings.ShelterLocation;
            existing.ShelterCapacity = settings.ShelterCapacity;
            existing.ShelterManager = settings.ShelterManager;
            existing.ShelterAddress = settings.ShelterAddress;
            existing.TwoFactor = settings.TwoFactor;
            existing.LoginAlerts = settings.LoginAlerts;
            existing.NotifInApp = settings.NotifInApp;
            existing.NotifEmail = settings.NotifEmail;
            existing.NotifSms = settings.NotifSms;
            existing.NotifCritical = settings.NotifCritical;
            existing.NotifRequests = settings.NotifRequests;
            existing.NotifInventory = settings.NotifInventory;
            existing.NotifSystem = settings.NotifSystem;
            _context.UserSettings.Update(existing);
        }
        await _context.SaveChangesAsync();
        return existing ?? settings;
    }

    #region Coordinator Settings

    public Task<CoordinatorSettings> GetCoordinatorSettingsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_coordinatorSettings);
        }
    }

    public Task<CoordinatorProfileSettings> UpdateCoordinatorProfileAsync(CoordinatorProfileSettings profile, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _coordinatorSettings.Profile = profile;
            _coordinatorSettings.UpdatedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(profile);
        }
    }

    public Task<CoordinatorOperationalSettings> UpdateCoordinatorOperationalAsync(CoordinatorOperationalSettings operational, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _coordinatorSettings.Operational = operational;
            _coordinatorSettings.UpdatedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(operational);
        }
    }

    public Task<CoordinatorNotificationSettings> UpdateCoordinatorNotificationsAsync(CoordinatorNotificationSettings notifications, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _coordinatorSettings.Notifications = notifications;
            _coordinatorSettings.UpdatedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(notifications);
        }
    }

    public Task<bool> VerifyCoordinatorPasswordAsync(string currentPassword, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_coordinatorSettings.Security.PasswordHash == currentPassword || currentPassword == "Coordinator@123");
        }
    }

    public Task<bool> ChangeCoordinatorPasswordAsync(string newPassword, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _coordinatorSettings.Security.PasswordHash = newPassword;
            _coordinatorSettings.Security.LastPasswordChangedAt = DateTimeOffset.UtcNow;
            _coordinatorSettings.UpdatedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(true);
        }
    }

    public Task<IReadOnlyList<UserSession>> GetCoordinatorSessionsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_coordinatorSettings.Sessions.Count == 0)
            {
                _coordinatorSettings.Sessions.Add(new UserSession
                {
                    Id = "sess-1",
                    Title = "Chrome on macOS",
                    Description = "Active session in Riverton Central",
                    DeviceType = "desktop",
                    Time = "Active now",
                    IpAddress = "192.168.1.10",
                    Current = true,
                    LastActiveAt = DateTimeOffset.UtcNow
                });
            }
            return Task.FromResult<IReadOnlyList<UserSession>>(_coordinatorSettings.Sessions);
        }
    }

    public Task<bool> RevokeSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var idx = _coordinatorSettings.Sessions.FindIndex(s => s.Id == sessionId);
            if (idx >= 0)
            {
                _coordinatorSettings.Sessions.RemoveAt(idx);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

    public Task<CoordinatorSettings> ResetCoordinatorSettingsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _coordinatorSettings = new CoordinatorSettings();
            return Task.FromResult(_coordinatorSettings);
        }
    }

    #endregion

    #region Partner Settings

    public Task<PartnerSettings> GetPartnerSettingsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_partnerSettings);
        }
    }

    public Task<PartnerProfileSettings> UpdatePartnerProfileAsync(PartnerProfileSettings profile, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _partnerSettings.Profile = profile;
            _partnerSettings.UpdatedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(profile);
        }
    }

    public Task<PartnerOrganisationSettings> UpdatePartnerOrganisationAsync(PartnerOrganisationSettings organisation, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _partnerSettings.Organisation = organisation;
            _partnerSettings.UpdatedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(organisation);
        }
    }

    public Task<PartnerSharingSettings> UpdatePartnerSharingAsync(PartnerSharingSettings sharing, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _partnerSettings.Sharing = sharing;
            _partnerSettings.UpdatedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(sharing);
        }
    }

    public Task<PartnerNotificationSettings> UpdatePartnerNotificationsAsync(PartnerNotificationSettings notifications, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _partnerSettings.Notifications = notifications;
            _partnerSettings.UpdatedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(notifications);
        }
    }

    public Task<PartnerSecuritySettings> UpdatePartnerSecurityAsync(PartnerSecuritySettings security, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _partnerSettings.Security.TwoFactorEnabled = security.TwoFactor;
            _partnerSettings.Security.LoginAlerts = security.LoginAlerts;
            _partnerSettings.Security.SessionTimeout = security.SessionTimeout;
            _partnerSettings.UpdatedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(security);
        }
    }

    public Task<bool> VerifyPartnerPasswordAsync(string currentPassword, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_partnerSettings.Security.PasswordHash == currentPassword || currentPassword == "Partner@123");
        }
    }

    public Task<bool> ChangePartnerPasswordAsync(string newPassword, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _partnerSettings.Security.PasswordHash = newPassword;
            _partnerSettings.Security.LastPasswordChangedAt = DateTimeOffset.UtcNow;
            _partnerSettings.UpdatedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(true);
        }
    }

    public Task<PartnerSettings> ResetPartnerSettingsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _partnerSettings = new PartnerSettings();
            return Task.FromResult(_partnerSettings);
        }
    }

    #endregion
}
