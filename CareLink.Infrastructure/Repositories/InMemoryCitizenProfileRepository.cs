namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryCitizenProfileRepository : ICitizenProfileRepository
{
    private readonly ConcurrentDictionary<string, CitizenProfileSettings> _profiles = new();
    private readonly Lock _lock = new();

    public InMemoryCitizenProfileRepository()
    {
        SeedDefaultProfile();
    }

    private void SeedDefaultProfile()
    {
        var defaultProfile = new CitizenProfileSettings
        {
            Id = "CPS-001",
            CitizenReference = "CIT-10482",
            CitizenName = "Samira Khan",
            PreferredName = "Samira",
            DateOfBirth = "1990-12-04",
            NationalId = "199084501234",
            Gender = "Female",
            Email = "samira.khan@email.com",
            Phone = "+94 77 412 8088",
            Address = "North Ward, Greater Riverton",
            District = "North Ward",
            City = "Greater Riverton",
            PostalCode = "10230",
            EmergencyContactName = "Rashid Khan",
            EmergencyContactPhone = "+94 77 455 0182",
            EmergencyContactRelation = "Brother",
            Language = "English",
            Accessibility = "Standard",
            HighContrast = false,
            LargeText = false,
            ScreenReaderSupport = false,
            AudioAssistance = false,
            EmergencyAlerts = true,
            RequestStatusUpdates = true,
            SafetyNotifications = true,
            MessagesFromSupportTeams = true,
            ShelterAvailabilityUpdates = true,
            SmsAlerts = true,
            EmailAlerts = true,
            ShareLocationDuringDisaster = true,
            ShareMedicalWithRescuers = true,
            AllowVolunteersContact = true,
            DataRetentionConsent = true,
            PersonalInformationPolicy = "Your personal information is protected and only shared with authorised support teams when necessary.",
            DataSharingPolicy = "Control how your information is used when requesting assistance.",
            PasswordHash = "Pass@1234",
            TwoFactorEnabled = true,
            LastPasswordChange = DateTime.UtcNow.AddDays(-30),
            LastLoginAt = DateTime.UtcNow,
            ActiveSessions = new List<SecuritySessionItem>
            {
                new() { Device = "Chrome on Windows 11 (Current)", Location = "Colombo / Greater Riverton", LastActive = "Just now", IsCurrent = true },
                new() { Device = "CareLink Mobile App on iOS 18", Location = "North Ward", LastActive = "3 hours ago", IsCurrent = false }
            },
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            UpdatedAt = DateTime.UtcNow
        };

        _profiles[defaultProfile.CitizenName.ToLowerInvariant()] = defaultProfile;
    }

    public Task<CitizenProfileSettings> GetProfileAsync(string? citizenName = null)
    {
        var key = (string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim()).ToLowerInvariant();

        if (!_profiles.TryGetValue(key, out var profile))
        {
            lock (_lock)
            {
                if (!_profiles.TryGetValue(key, out profile))
                {
                    profile = new CitizenProfileSettings
                    {
                        Id = $"CPS-{_profiles.Count + 1:D3}",
                        CitizenReference = $"CIT-{10000 + _profiles.Count + 1}",
                        CitizenName = string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim(),
                        PreferredName = string.IsNullOrWhiteSpace(citizenName) ? "Samira" : citizenName.Trim().Split(' ')[0],
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _profiles[key] = profile;
                }
            }
        }

        return Task.FromResult(CloneProfile(profile));
    }

    public Task<CitizenProfileSettings> SaveProfileAsync(CitizenProfileSettings profile)
    {
        lock (_lock)
        {
            var key = profile.CitizenName.Trim().ToLowerInvariant();
            profile.UpdatedAt = DateTime.UtcNow;
            _profiles[key] = profile;
            return Task.FromResult(CloneProfile(profile));
        }
    }

    public Task<bool> VerifyPasswordAsync(string citizenName, string currentPassword)
    {
        var key = (string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim()).ToLowerInvariant();
        if (_profiles.TryGetValue(key, out var profile))
        {
            // For in-memory demo, match simulated password or allow "Pass@1234"
            var isValid = string.Equals(profile.PasswordHash, currentPassword) || currentPassword == "Pass@1234" || currentPassword == "password";
            return Task.FromResult(isValid);
        }
        return Task.FromResult(false);
    }

    public Task<bool> ChangePasswordAsync(string citizenName, string newPassword)
    {
        lock (_lock)
        {
            var key = (string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim()).ToLowerInvariant();
            if (_profiles.TryGetValue(key, out var profile))
            {
                profile.PasswordHash = newPassword;
                profile.LastPasswordChange = DateTime.UtcNow;
                profile.UpdatedAt = DateTime.UtcNow;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

    public Task<CitizenProfileSettings> ResetToDefaultAsync(string? citizenName = null)
    {
        lock (_lock)
        {
            var key = (string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim()).ToLowerInvariant();
            _profiles.TryRemove(key, out _);
            SeedDefaultProfile();
            return GetProfileAsync(citizenName);
        }
    }

    private static CitizenProfileSettings CloneProfile(CitizenProfileSettings source)
    {
        return new CitizenProfileSettings
        {
            Id = source.Id,
            CitizenReference = source.CitizenReference,
            CitizenName = source.CitizenName,
            PreferredName = source.PreferredName,
            DateOfBirth = source.DateOfBirth,
            NationalId = source.NationalId,
            Gender = source.Gender,
            Email = source.Email,
            Phone = source.Phone,
            Address = source.Address,
            District = source.District,
            City = source.City,
            PostalCode = source.PostalCode,
            EmergencyContactName = source.EmergencyContactName,
            EmergencyContactPhone = source.EmergencyContactPhone,
            EmergencyContactRelation = source.EmergencyContactRelation,
            Language = source.Language,
            Accessibility = source.Accessibility,
            HighContrast = source.HighContrast,
            LargeText = source.LargeText,
            ScreenReaderSupport = source.ScreenReaderSupport,
            AudioAssistance = source.AudioAssistance,
            EmergencyAlerts = source.EmergencyAlerts,
            RequestStatusUpdates = source.RequestStatusUpdates,
            SafetyNotifications = source.SafetyNotifications,
            MessagesFromSupportTeams = source.MessagesFromSupportTeams,
            ShelterAvailabilityUpdates = source.ShelterAvailabilityUpdates,
            SmsAlerts = source.SmsAlerts,
            EmailAlerts = source.EmailAlerts,
            ShareLocationDuringDisaster = source.ShareLocationDuringDisaster,
            ShareMedicalWithRescuers = source.ShareMedicalWithRescuers,
            AllowVolunteersContact = source.AllowVolunteersContact,
            DataRetentionConsent = source.DataRetentionConsent,
            PersonalInformationPolicy = source.PersonalInformationPolicy,
            DataSharingPolicy = source.DataSharingPolicy,
            PasswordHash = source.PasswordHash,
            TwoFactorEnabled = source.TwoFactorEnabled,
            LastPasswordChange = source.LastPasswordChange,
            LastLoginAt = source.LastLoginAt,
            ActiveSessions = source.ActiveSessions.Select(s => new SecuritySessionItem
            {
                Device = s.Device,
                Location = s.Location,
                LastActive = s.LastActive,
                IsCurrent = s.IsCurrent
            }).ToList(),
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }
}
