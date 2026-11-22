using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemorySettingsRepository : ISettingsRepository
{
    private readonly object _lock = new();
    private CoordinatorSettings _coordinatorSettings;
    private PartnerSettings _partnerSettings;

    public InMemorySettingsRepository()
    {
        _coordinatorSettings = CreateDefaultCoordinatorSettings();
        _partnerSettings = CreateDefaultPartnerSettings();
    }

    #region Coordinator Settings

    public Task<CoordinatorSettings> GetCoordinatorSettingsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(CloneCoordinatorSettings(_coordinatorSettings));
        }
    }

    public Task<CoordinatorProfileSettings> UpdateCoordinatorProfileAsync(
        CoordinatorProfileSettings profile,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _coordinatorSettings.Profile.FullName = profile.FullName.Trim();
            _coordinatorSettings.Profile.Email = profile.Email.Trim();
            _coordinatorSettings.Profile.Phone = profile.Phone.Trim();
            _coordinatorSettings.Profile.Area = profile.Area.Trim();
            _coordinatorSettings.Profile.Role = string.IsNullOrWhiteSpace(profile.Role) ? "Emergency Coordinator" : profile.Role.Trim();
            _coordinatorSettings.Profile.Initials = ComputeInitials(_coordinatorSettings.Profile.FullName);
            _coordinatorSettings.UpdatedAt = DateTimeOffset.UtcNow;

            return Task.FromResult(CloneCoordinatorProfile(_coordinatorSettings.Profile));
        }
    }

    public Task<CoordinatorOperationalSettings> UpdateCoordinatorOperationalAsync(
        CoordinatorOperationalSettings operational,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _coordinatorSettings.Operational.AutoRefresh = operational.AutoRefresh;
            _coordinatorSettings.Operational.AiRecommendations = operational.AiRecommendations;
            _coordinatorSettings.Operational.CriticalAlerts = operational.CriticalAlerts;
            _coordinatorSettings.Operational.LiveMap = operational.LiveMap;
            _coordinatorSettings.Operational.DefaultPriority = operational.DefaultPriority ?? "High";
            _coordinatorSettings.Operational.RefreshInterval = operational.RefreshInterval ?? "30 seconds";
            _coordinatorSettings.UpdatedAt = DateTimeOffset.UtcNow;

            return Task.FromResult(CloneCoordinatorOperational(_coordinatorSettings.Operational));
        }
    }

    public Task<CoordinatorNotificationSettings> UpdateCoordinatorNotificationsAsync(
        CoordinatorNotificationSettings notifications,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _coordinatorSettings.Notifications.Critical = notifications.Critical;
            _coordinatorSettings.Notifications.Ai = notifications.Ai;
            _coordinatorSettings.Notifications.Resources = notifications.Resources;
            _coordinatorSettings.Notifications.Shelters = notifications.Shelters;
            _coordinatorSettings.Notifications.Partners = notifications.Partners;
            _coordinatorSettings.Notifications.Reports = notifications.Reports;
            _coordinatorSettings.Notifications.Email = notifications.Email;
            _coordinatorSettings.Notifications.Browser = notifications.Browser;
            _coordinatorSettings.UpdatedAt = DateTimeOffset.UtcNow;

            return Task.FromResult(CloneCoordinatorNotifications(_coordinatorSettings.Notifications));
        }
    }

    public Task<bool> VerifyCoordinatorPasswordAsync(string currentPassword, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(currentPassword)) return Task.FromResult(false);
            // Accepts the stored password, or initial fallback "Coordinator@123"
            bool matches = string.Equals(_coordinatorSettings.Security.PasswordHash, currentPassword, StringComparison.Ordinal);
            return Task.FromResult(matches);
        }
    }

    public Task<bool> ChangeCoordinatorPasswordAsync(string newPassword, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                return Task.FromResult(false);
            }

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
            var list = _coordinatorSettings.Sessions.Select(CloneSession).ToList();
            return Task.FromResult<IReadOnlyList<UserSession>>(list);
        }
    }

    public Task<bool> RevokeSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var session = _coordinatorSettings.Sessions.FirstOrDefault(s => s.Id.Equals(sessionId, StringComparison.OrdinalIgnoreCase));
            if (session == null || session.Current)
            {
                return Task.FromResult(false); // Cannot revoke active current session or non-existent
            }

            _coordinatorSettings.Sessions.Remove(session);
            _coordinatorSettings.UpdatedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(true);
        }
    }

    public Task<CoordinatorSettings> ResetCoordinatorSettingsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _coordinatorSettings = CreateDefaultCoordinatorSettings();
            return Task.FromResult(CloneCoordinatorSettings(_coordinatorSettings));
        }
    }

    #endregion

    #region Partner Settings

    public Task<PartnerSettings> GetPartnerSettingsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(ClonePartnerSettings(_partnerSettings));
        }
    }

    public Task<PartnerProfileSettings> UpdatePartnerProfileAsync(
        PartnerProfileSettings profile,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _partnerSettings.Profile.FullName = profile.FullName.Trim();
            _partnerSettings.Profile.Email = profile.Email.Trim();
            _partnerSettings.Profile.Phone = profile.Phone.Trim();
            _partnerSettings.Profile.Role = string.IsNullOrWhiteSpace(profile.Role) ? "Partner Officer" : profile.Role.Trim();
            _partnerSettings.UpdatedAt = DateTimeOffset.UtcNow;

            return Task.FromResult(ClonePartnerProfile(_partnerSettings.Profile));
        }
    }

    public Task<PartnerOrganisationSettings> UpdatePartnerOrganisationAsync(
        PartnerOrganisationSettings organisation,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _partnerSettings.Organisation.Name = organisation.Name.Trim();
            _partnerSettings.Organisation.Registration = organisation.Registration.Trim();
            _partnerSettings.Organisation.Type = organisation.Type.Trim();
            _partnerSettings.Organisation.Location = organisation.Location.Trim();
            _partnerSettings.Organisation.Timezone = organisation.Timezone.Trim();
            _partnerSettings.Organisation.Language = organisation.Language.Trim();
            _partnerSettings.Organisation.Currency = organisation.Currency.Trim();
            _partnerSettings.UpdatedAt = DateTimeOffset.UtcNow;

            return Task.FromResult(ClonePartnerOrganisation(_partnerSettings.Organisation));
        }
    }

    public Task<PartnerSharingSettings> UpdatePartnerSharingAsync(
        PartnerSharingSettings sharing,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _partnerSettings.Sharing.Referrals = sharing.Referrals;
            _partnerSettings.Sharing.Resources = sharing.Resources;
            _partnerSettings.Sharing.Activities = sharing.Activities;
            _partnerSettings.Sharing.Reports = sharing.Reports;
            _partnerSettings.Sharing.Documents = sharing.Documents;
            _partnerSettings.Sharing.PublicProfile = sharing.PublicProfile;
            _partnerSettings.UpdatedAt = DateTimeOffset.UtcNow;

            return Task.FromResult(ClonePartnerSharing(_partnerSettings.Sharing));
        }
    }

    public Task<PartnerNotificationSettings> UpdatePartnerNotificationsAsync(
        PartnerNotificationSettings notifications,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _partnerSettings.Notifications.Urgent = notifications.Urgent;
            _partnerSettings.Notifications.Referrals = notifications.Referrals;
            _partnerSettings.Notifications.Collaboration = notifications.Collaboration;
            _partnerSettings.Notifications.Resources = notifications.Resources;
            _partnerSettings.Notifications.Reports = notifications.Reports;
            _partnerSettings.Notifications.Team = notifications.Team;
            _partnerSettings.Notifications.Email = notifications.Email;
            _partnerSettings.Notifications.Push = notifications.Push;
            _partnerSettings.UpdatedAt = DateTimeOffset.UtcNow;

            return Task.FromResult(ClonePartnerNotifications(_partnerSettings.Notifications));
        }
    }

    public Task<PartnerSecuritySettings> UpdatePartnerSecurityAsync(
        PartnerSecuritySettings security,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _partnerSettings.Security.TwoFactorEnabled = security.TwoFactor;
            _partnerSettings.Security.LoginAlerts = security.LoginAlerts;
            _partnerSettings.Security.SessionTimeout = security.SessionTimeout ?? "30 minutes";
            _partnerSettings.UpdatedAt = DateTimeOffset.UtcNow;

            return Task.FromResult(ClonePartnerSecurity(_partnerSettings.Security));
        }
    }

    public Task<bool> VerifyPartnerPasswordAsync(string currentPassword, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(currentPassword)) return Task.FromResult(false);
            bool matches = string.Equals(_partnerSettings.Security.PasswordHash, currentPassword, StringComparison.Ordinal);
            return Task.FromResult(matches);
        }
    }

    public Task<bool> ChangePartnerPasswordAsync(string newPassword, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                return Task.FromResult(false);
            }

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
            _partnerSettings = CreateDefaultPartnerSettings();
            return Task.FromResult(ClonePartnerSettings(_partnerSettings));
        }
    }

    #endregion

    #region Factory Defaults & Cloners

    private static CoordinatorSettings CreateDefaultCoordinatorSettings()
    {
        return new CoordinatorSettings
        {
            Profile = new CoordinatorProfileSettings
            {
                FullName = "Amina Mensah",
                Email = "amina@response.org",
                Phone = "+94 77 412 8041",
                Area = "Greater Riverton",
                Role = "Emergency Coordinator",
                Initials = "AM",
                BadgeNumber = "EC-8842"
            },
            Operational = new CoordinatorOperationalSettings
            {
                AutoRefresh = true,
                AiRecommendations = true,
                CriticalAlerts = true,
                LiveMap = true,
                DefaultPriority = "High",
                RefreshInterval = "30 seconds"
            },
            Notifications = new CoordinatorNotificationSettings
            {
                Critical = true,
                Ai = true,
                Resources = true,
                Shelters = true,
                Partners = true,
                Reports = true,
                Email = true,
                Browser = true
            },
            Security = new SecuritySettings
            {
                PasswordHash = "Coordinator@123",
                TwoFactorEnabled = true,
                LoginAlerts = true,
                SessionTimeout = "30 minutes",
                LastPasswordChangedAt = DateTimeOffset.UtcNow.AddDays(-14)
            },
            Sessions = new List<UserSession>
            {
                new()
                {
                    Id = "sess-001",
                    Title = "Windows - Chrome",
                    Description = "Current session · Windows desktop",
                    DeviceType = "desktop",
                    Time = "Active now",
                    IpAddress = "192.168.1.100",
                    Current = true,
                    LastActiveAt = DateTimeOffset.UtcNow
                },
                new()
                {
                    Id = "sess-002",
                    Title = "Mobile device",
                    Description = "Previously logged-in mobile device",
                    DeviceType = "mobile",
                    Time = "Last active 2 hours ago",
                    IpAddress = "172.56.21.8",
                    Current = false,
                    LastActiveAt = DateTimeOffset.UtcNow.AddHours(-2)
                },
                new()
                {
                    Id = "sess-003",
                    Title = "Field HQ Terminal",
                    Description = "Emergency command center workstation",
                    DeviceType = "desktop",
                    Time = "Last active 1 day ago",
                    IpAddress = "192.168.1.145",
                    Current = false,
                    LastActiveAt = DateTimeOffset.UtcNow.AddDays(-1)
                }
            },
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    private static PartnerSettings CreateDefaultPartnerSettings()
    {
        return new PartnerSettings
        {
            Profile = new PartnerProfileSettings
            {
                FullName = "Sara Khan",
                Email = "sara@globalrelief.org",
                Phone = "+94 77 412 8014",
                Role = "Partner Officer"
            },
            Organisation = new PartnerOrganisationSettings
            {
                Name = "Global Relief Network",
                Registration = "ORG-027",
                Type = "International NGO",
                Location = "Central Ward, Greater Riverton",
                Timezone = "Asia/Colombo",
                Language = "English",
                Currency = "LKR"
            },
            Sharing = new PartnerSharingSettings
            {
                Referrals = true,
                Resources = true,
                Activities = true,
                Reports = false,
                Documents = true,
                PublicProfile = true
            },
            Notifications = new PartnerNotificationSettings
            {
                Urgent = true,
                Referrals = true,
                Collaboration = true,
                Resources = true,
                Reports = true,
                Team = false,
                Email = true,
                Push = true
            },
            Security = new SecuritySettings
            {
                PasswordHash = "Partner@123",
                TwoFactorEnabled = true,
                LoginAlerts = true,
                SessionTimeout = "30 minutes",
                LastPasswordChangedAt = DateTimeOffset.UtcNow.AddDays(-20)
            },
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    private static CoordinatorSettings CloneCoordinatorSettings(CoordinatorSettings s)
    {
        return new CoordinatorSettings
        {
            Profile = CloneCoordinatorProfile(s.Profile),
            Operational = CloneCoordinatorOperational(s.Operational),
            Notifications = CloneCoordinatorNotifications(s.Notifications),
            Security = CloneSecurity(s.Security),
            Sessions = s.Sessions.Select(CloneSession).ToList(),
            UpdatedAt = s.UpdatedAt
        };
    }

    private static CoordinatorProfileSettings CloneCoordinatorProfile(CoordinatorProfileSettings p)
    {
        return new CoordinatorProfileSettings
        {
            FullName = p.FullName,
            Email = p.Email,
            Phone = p.Phone,
            Area = p.Area,
            Role = p.Role,
            Initials = p.Initials,
            BadgeNumber = p.BadgeNumber
        };
    }

    private static CoordinatorOperationalSettings CloneCoordinatorOperational(CoordinatorOperationalSettings o)
    {
        return new CoordinatorOperationalSettings
        {
            AutoRefresh = o.AutoRefresh,
            AiRecommendations = o.AiRecommendations,
            CriticalAlerts = o.CriticalAlerts,
            LiveMap = o.LiveMap,
            DefaultPriority = o.DefaultPriority,
            RefreshInterval = o.RefreshInterval
        };
    }

    private static CoordinatorNotificationSettings CloneCoordinatorNotifications(CoordinatorNotificationSettings n)
    {
        return new CoordinatorNotificationSettings
        {
            Critical = n.Critical,
            Ai = n.Ai,
            Resources = n.Resources,
            Shelters = n.Shelters,
            Partners = n.Partners,
            Reports = n.Reports,
            Email = n.Email,
            Browser = n.Browser
        };
    }

    private static SecuritySettings CloneSecurity(SecuritySettings sec)
    {
        return new SecuritySettings
        {
            PasswordHash = sec.PasswordHash,
            TwoFactorEnabled = sec.TwoFactorEnabled,
            LoginAlerts = sec.LoginAlerts,
            SessionTimeout = sec.SessionTimeout,
            LastPasswordChangedAt = sec.LastPasswordChangedAt
        };
    }

    private static UserSession CloneSession(UserSession s)
    {
        return new UserSession
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            DeviceType = s.DeviceType,
            Time = s.Time,
            IpAddress = s.IpAddress,
            Current = s.Current,
            LastActiveAt = s.LastActiveAt
        };
    }

    private static PartnerSettings ClonePartnerSettings(PartnerSettings s)
    {
        return new PartnerSettings
        {
            Profile = ClonePartnerProfile(s.Profile),
            Organisation = ClonePartnerOrganisation(s.Organisation),
            Sharing = ClonePartnerSharing(s.Sharing),
            Notifications = ClonePartnerNotifications(s.Notifications),
            Security = CloneSecurity(s.Security),
            UpdatedAt = s.UpdatedAt
        };
    }

    private static PartnerProfileSettings ClonePartnerProfile(PartnerProfileSettings p)
    {
        return new PartnerProfileSettings
        {
            FullName = p.FullName,
            Email = p.Email,
            Phone = p.Phone,
            Role = p.Role
        };
    }

    private static PartnerOrganisationSettings ClonePartnerOrganisation(PartnerOrganisationSettings o)
    {
        return new PartnerOrganisationSettings
        {
            Name = o.Name,
            Registration = o.Registration,
            Type = o.Type,
            Location = o.Location,
            Timezone = o.Timezone,
            Language = o.Language,
            Currency = o.Currency
        };
    }

    private static PartnerSharingSettings ClonePartnerSharing(PartnerSharingSettings s)
    {
        return new PartnerSharingSettings
        {
            Referrals = s.Referrals,
            Resources = s.Resources,
            Activities = s.Activities,
            Reports = s.Reports,
            Documents = s.Documents,
            PublicProfile = s.PublicProfile
        };
    }

    private static PartnerNotificationSettings ClonePartnerNotifications(PartnerNotificationSettings n)
    {
        return new PartnerNotificationSettings
        {
            Urgent = n.Urgent,
            Referrals = n.Referrals,
            Collaboration = n.Collaboration,
            Resources = n.Resources,
            Reports = n.Reports,
            Team = n.Team,
            Email = n.Email,
            Push = n.Push
        };
    }

    private static PartnerSecuritySettings ClonePartnerSecurity(SecuritySettings s)
    {
        return new PartnerSecuritySettings
        {
            TwoFactor = s.TwoFactorEnabled,
            LoginAlerts = s.LoginAlerts,
            SessionTimeout = s.SessionTimeout
        };
    }

    private static string ComputeInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "EC";
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0][0].ToString().ToUpperInvariant();
        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }

    #endregion
}
