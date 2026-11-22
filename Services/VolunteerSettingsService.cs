using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class VolunteerSettingsService : IVolunteerSettingsService
{
    private readonly object _lock = new();
    private VolunteerSettingsDto _settings;

    public VolunteerSettingsService()
    {
        _settings = CreateDefaultSettings();
    }

    public Task<VolunteerSettingsDto> GetSettingsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(CloneSettings(_settings));
        }
    }

    public Task<VolunteerSettingsDto> UpdateSettingsAsync(VolunteerSettingsDto newSettings)
    {
        lock (_lock)
        {
            _settings.Profile = newSettings.Profile ?? _settings.Profile;
            _settings.MatchingRules = newSettings.MatchingRules ?? _settings.MatchingRules;
            _settings.NotificationSettings = newSettings.NotificationSettings ?? _settings.NotificationSettings;
            _settings.Security = newSettings.Security ?? _settings.Security;

            if (newSettings.ActiveSessions != null && newSettings.ActiveSessions.Count > 0)
            {
                _settings.ActiveSessions = newSettings.ActiveSessions;
            }

            return Task.FromResult(CloneSettings(_settings));
        }
    }

    public Task<bool> ChangePasswordAsync(VolunteerChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            return Task.FromResult(false);
        }

        if (!string.IsNullOrEmpty(request.ConfirmPassword) && request.NewPassword != request.ConfirmPassword)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public Task<bool> RevokeSessionAsync(string sessionId)
    {
        lock (_lock)
        {
            var session = _settings.ActiveSessions.FirstOrDefault(s => s.Id.Equals(sessionId, StringComparison.OrdinalIgnoreCase));
            if (session == null || session.IsCurrent)
            {
                return Task.FromResult(false);
            }

            _settings.ActiveSessions.Remove(session);
            return Task.FromResult(true);
        }
    }

    public Task<VolunteerSettingsDto> ResetDefaultsAsync()
    {
        lock (_lock)
        {
            _settings = CreateDefaultSettings();
            return Task.FromResult(CloneSettings(_settings));
        }
    }

    private static VolunteerSettingsDto CreateDefaultSettings()
    {
        return new VolunteerSettingsDto
        {
            Profile = new VolunteerProfileDto
            {
                FullName = "Nadia Okafor",
                Email = "nadia@reliefos.org",
                Phone = "+94 77 412 8041",
                Area = "Greater Riverton",
                Photo = null
            },
            MatchingRules = new AIMatchingRulesDto
            {
                MinimumScore = "70",
                TravelDistance = "25",
                Availability = true,
                VerifiedSkills = true,
                Experience = true,
                Training = true,
                PreventConflicts = true,
                HumanApproval = true
            },
            NotificationSettings = new VolunteerNotificationPreferencesDto
            {
                Assignments = true,
                Verification = true,
                Safety = true,
                AiMatching = true,
                Communications = true,
                InApp = true,
                Email = true,
                Sms = false,
                UrgentOnly = false
            },
            Security = new VolunteerSecuritySettingsDto
            {
                TwoFactor = true
            },
            ActiveSessions =
            [
                new VolunteerActiveSessionDto
                {
                    Id = "sess-01",
                    Device = "Windows",
                    Browser = "Chrome",
                    Location = "Riverton Operations Center",
                    LastActive = "Active now",
                    IsCurrent = true
                },
                new VolunteerActiveSessionDto
                {
                    Id = "sess-02",
                    Device = "Android",
                    Browser = "CareLink Mobile",
                    Location = "Riverton Field Office",
                    LastActive = "2 hours ago",
                    IsCurrent = false
                }
            ]
        };
    }

    private static VolunteerSettingsDto CloneSettings(VolunteerSettingsDto s)
    {
        return new VolunteerSettingsDto
        {
            Profile = new VolunteerProfileDto
            {
                FullName = s.Profile.FullName,
                Email = s.Profile.Email,
                Phone = s.Profile.Phone,
                Area = s.Profile.Area,
                Photo = s.Profile.Photo
            },
            MatchingRules = new AIMatchingRulesDto
            {
                MinimumScore = s.MatchingRules.MinimumScore,
                TravelDistance = s.MatchingRules.TravelDistance,
                Availability = s.MatchingRules.Availability,
                VerifiedSkills = s.MatchingRules.VerifiedSkills,
                Experience = s.MatchingRules.Experience,
                Training = s.MatchingRules.Training,
                PreventConflicts = s.MatchingRules.PreventConflicts,
                HumanApproval = s.MatchingRules.HumanApproval
            },
            NotificationSettings = new VolunteerNotificationPreferencesDto
            {
                Assignments = s.NotificationSettings.Assignments,
                Verification = s.NotificationSettings.Verification,
                Safety = s.NotificationSettings.Safety,
                AiMatching = s.NotificationSettings.AiMatching,
                Communications = s.NotificationSettings.Communications,
                InApp = s.NotificationSettings.InApp,
                Email = s.NotificationSettings.Email,
                Sms = s.NotificationSettings.Sms,
                UrgentOnly = s.NotificationSettings.UrgentOnly
            },
            Security = new VolunteerSecuritySettingsDto
            {
                TwoFactor = s.Security.TwoFactor
            },
            ActiveSessions = s.ActiveSessions.Select(a => new VolunteerActiveSessionDto
            {
                Id = a.Id,
                Device = a.Device,
                Browser = a.Browser,
                Location = a.Location,
                LastActive = a.LastActive,
                IsCurrent = a.IsCurrent
            }).ToList()
        };
    }
}
