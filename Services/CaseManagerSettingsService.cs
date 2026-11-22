using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class CaseManagerSettingsService : ICaseManagerSettingsService
{
    private CaseManagerSettingsDto _settings;
    private readonly object _lock = new();

    public CaseManagerSettingsService()
    {
        _settings = CreateDefaultSettings();
    }

    private static CaseManagerSettingsDto CreateDefaultSettings()
    {
        return new CaseManagerSettingsDto
        {
            Profile = new UserProfileSettingsDto
            {
                FullName = "Amina Mensah",
                Email = "amina@response.org",
                Phone = "+94 77 412 8041",
                Area = "Greater Riverton"
            },
            Queue = new QueuePreferencesDto
            {
                DefaultPriority = "All priorities",
                DefaultCategory = "All categories",
                DefaultStatus = "All statuses",
                AutoRefresh = true,
                ShowCriticalFirst = true,
                ShowMyCasesOnly = false
            },
            Notifications = new NotificationPreferencesSettingsDto
            {
                InAppNotifications = true,
                EmailNotifications = true,
                PushNotifications = true,
                CriticalAlerts = true,
                AssignmentAlerts = true,
                EvidenceUpdates = true,
                EscalationAlerts = true,
                AiRecommendations = false
            },
            Security = new SecuritySettingsDto
            {
                TwoFactor = true,
                LoginAlerts = true
            },
            ActiveSessions =
            [
                new()
                {
                    Id = "sess-01",
                    Device = "Windows",
                    Browser = "Chrome",
                    Location = "Colombo, Western Province",
                    LastActive = "Active now",
                    IsCurrent = true
                },
                new()
                {
                    Id = "sess-02",
                    Device = "Android",
                    Browser = "Chrome Mobile",
                    Location = "Riverton Field Office",
                    LastActive = "2 hours ago",
                    IsCurrent = false
                }
            ]
        };
    }

    public Task<CaseManagerSettingsDto> GetSettingsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_settings);
        }
    }

    public Task<CaseManagerSettingsDto> UpdateSettingsAsync(CaseManagerSettingsDto settings)
    {
        lock (_lock)
        {
            _settings = settings;
            return Task.FromResult(_settings);
        }
    }

    public Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return Task.FromResult(false);
            }
            if (request.NewPassword.Length < 8)
            {
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }
    }

    public Task<bool> RevokeSessionAsync(string sessionId)
    {
        lock (_lock)
        {
            var session = _settings.ActiveSessions.FirstOrDefault(s => s.Id.Equals(sessionId, StringComparison.OrdinalIgnoreCase) || s.Device.Equals(sessionId, StringComparison.OrdinalIgnoreCase));
            if (session != null && !session.IsCurrent)
            {
                _settings.ActiveSessions.Remove(session);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

    public Task<CaseManagerSettingsDto> ResetDefaultsAsync()
    {
        lock (_lock)
        {
            _settings = CreateDefaultSettings();
            return Task.FromResult(_settings);
        }
    }
}
