using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.Settings;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CareLink.Application.Services;

public class SettingsService : ISettingsService
{
    private SystemSettingsDto _settings = new();
    private readonly object _lock = new();
    private readonly ISettingsRepository _repository;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(
        ISettingsRepository repository,
        ILogger<SettingsService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    #region Platform System Settings (Admin / System Admin)

    public Task<ApiResponse<SystemSettingsDto>> GetSettingsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(ApiResponse<SystemSettingsDto>.SuccessResult(_settings));
        }
    }

    public Task<ApiResponse<SystemSettingsDto>> UpdateSettingsAsync(UpdateSystemSettingsDto request, string actor)
    {
        lock (_lock)
        {
            foreach (var kvp in request.Settings)
            {
                var val = kvp.Value;
                switch (kvp.Key.ToLowerInvariant())
                {
                    case "platformname":
                        if (val is string pname && !string.IsNullOrWhiteSpace(pname)) _settings.PlatformName = pname;
                        break;
                    case "language":
                        if (val is string lang) _settings.Language = lang;
                        break;
                    case "timezone":
                        if (val is string tz) _settings.Timezone = tz;
                        break;
                    case "supportemail":
                        if (val is string email && email.Contains('@')) _settings.SupportEmail = email;
                        break;
                    case "maintenancenotice":
                        if (val is string notice) _settings.MaintenanceNotice = notice;
                        break;
                    case "twofactor":
                        if (bool.TryParse(val.ToString(), out var b2fa)) _settings.TwoFactor = b2fa;
                        break;
                    case "compromisedpasswords":
                        if (bool.TryParse(val.ToString(), out var bcp)) _settings.CompromisedPasswords = bcp;
                        break;
                    case "suspicioussession":
                        if (bool.TryParse(val.ToString(), out var bss)) _settings.SuspiciousSession = bss;
                        break;
                    case "adminapproval":
                        if (bool.TryParse(val.ToString(), out var baa)) _settings.AdminApproval = baa;
                        break;
                    case "sessiontimeout":
                        if (val is string st) _settings.SessionTimeout = st;
                        break;
                    case "failedattempts":
                        if (val is string fa) _settings.FailedAttempts = fa;
                        break;
                    case "passwordlength":
                        if (val is string pl) _settings.PasswordLength = pl;
                        break;
                    case "resetexpiry":
                        if (val is string re) _settings.ResetExpiry = re;
                        break;
                    case "partnersso":
                        if (bool.TryParse(val.ToString(), out var bpsso)) _settings.PartnerSSO = bpsso;
                        break;
                    case "securityalerts":
                        if (bool.TryParse(val.ToString(), out var bsa)) _settings.SecurityAlerts = bsa;
                        break;
                    case "integrationalerts":
                        if (bool.TryParse(val.ToString(), out var bia)) _settings.IntegrationAlerts = bia;
                        break;
                    case "rolerequests":
                        if (bool.TryParse(val.ToString(), out var brr)) _settings.RoleRequests = brr;
                        break;
                    case "failedjobs":
                        if (bool.TryParse(val.ToString(), out var bfj)) _settings.FailedJobs = bfj;
                        break;
                    case "dailysummary":
                        if (bool.TryParse(val.ToString(), out var bds)) _settings.DailySummary = bds;
                        break;
                    case "auditretention":
                        if (val is string ar) _settings.AuditRetention = ar;
                        break;
                    case "inactiveretention":
                        if (val is string ir) _settings.InactiveRetention = ir;
                        break;
                    case "exportexpiry":
                        if (val is string ee) _settings.ExportExpiry = ee;
                        break;
                    case "logaggregation":
                        if (val is string la) _settings.LogAggregation = la;
                        break;
                    case "aidecision":
                        if (bool.TryParse(val.ToString(), out var baid)) _settings.AiDecision = baid;
                        break;
                    case "citizenregistration":
                        if (bool.TryParse(val.ToString(), out var bcr)) _settings.CitizenRegistration = bcr;
                        break;
                    case "partnersharing":
                        if (bool.TryParse(val.ToString(), out var bps)) _settings.PartnerSharing = bps;
                        break;
                    case "smsdelivery":
                        if (bool.TryParse(val.ToString(), out var bsms)) _settings.SmsDelivery = bsms;
                        break;
                    case "demandforecasts":
                        if (bool.TryParse(val.ToString(), out var bdf)) _settings.DemandForecasts = bdf;
                        break;
                }
            }

            _settings.LastUpdatedAt = DateTime.UtcNow;
            _settings.LastUpdatedBy = actor;

            _logger.LogInformation("System settings updated by {Actor}", actor);
            return Task.FromResult(ApiResponse<SystemSettingsDto>.SuccessResult(_settings, "Settings saved successfully."));
        }
    }

    public Task<ApiResponse<SystemSettingsDto>> ResetDefaultsAsync(string actor)
    {
        lock (_lock)
        {
            _settings = new SystemSettingsDto
            {
                LastUpdatedAt = DateTime.UtcNow,
                LastUpdatedBy = actor
            };

            _logger.LogInformation("System settings reset to defaults by {Actor}", actor);
            return Task.FromResult(ApiResponse<SystemSettingsDto>.SuccessResult(_settings, "Settings restored to defaults."));
        }
    }

    #endregion

    #region Coordinator Settings

    public async Task<CoordinatorSettingsDto> GetCoordinatorSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetCoordinatorSettingsAsync(cancellationToken);
        return MapToCoordinatorDto(settings);
    }

    public async Task<CoordinatorProfileSettingsDto> UpdateCoordinatorProfileAsync(
        UpdateCoordinatorProfileSettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new CoordinatorProfileSettings
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Area = dto.Area,
            Role = dto.Role ?? "Emergency Coordinator"
        };

        var updated = await _repository.UpdateCoordinatorProfileAsync(entity, cancellationToken);
        return MapToProfileDto(updated);
    }

    public async Task<CoordinatorOperationalSettingsDto> UpdateCoordinatorOperationalAsync(
        UpdateCoordinatorOperationalSettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new CoordinatorOperationalSettings
        {
            AutoRefresh = dto.AutoRefresh,
            AiRecommendations = dto.AiRecommendations,
            CriticalAlerts = dto.CriticalAlerts,
            LiveMap = dto.LiveMap,
            DefaultPriority = dto.DefaultPriority,
            RefreshInterval = dto.RefreshInterval
        };

        var updated = await _repository.UpdateCoordinatorOperationalAsync(entity, cancellationToken);
        return MapToOperationalDto(updated);
    }

    public async Task<CoordinatorNotificationSettingsDto> UpdateCoordinatorNotificationsAsync(
        UpdateCoordinatorNotificationSettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new CoordinatorNotificationSettings
        {
            Critical = dto.Critical,
            Ai = dto.Ai,
            Resources = dto.Resources,
            Shelters = dto.Shelters,
            Partners = dto.Partners,
            Reports = dto.Reports,
            Email = dto.Email,
            Browser = dto.Browser
        };

        var updated = await _repository.UpdateCoordinatorNotificationsAsync(entity, cancellationToken);
        return MapToNotificationsDto(updated);
    }

    public async Task<ChangePasswordResponseDto> ChangeCoordinatorPasswordAsync(
        ChangePasswordRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
        {
            return new ChangePasswordResponseDto
            {
                Success = false,
                Message = "New password and confirmation do not match."
            };
        }

        if (dto.NewPassword.Length < 6)
        {
            return new ChangePasswordResponseDto
            {
                Success = false,
                Message = "New password must contain at least 6 characters."
            };
        }

        bool verified = await _repository.VerifyCoordinatorPasswordAsync(dto.CurrentPassword, cancellationToken);
        if (!verified)
        {
            return new ChangePasswordResponseDto
            {
                Success = false,
                Message = "Current password is incorrect."
            };
        }

        bool changed = await _repository.ChangeCoordinatorPasswordAsync(dto.NewPassword, cancellationToken);
        if (!changed)
        {
            return new ChangePasswordResponseDto
            {
                Success = false,
                Message = "Failed to update password. Please try again."
            };
        }

        return new ChangePasswordResponseDto
        {
            Success = true,
            Message = "Password has been updated successfully."
        };
    }

    public async Task<IReadOnlyList<UserSessionDto>> GetCoordinatorSessionsAsync(CancellationToken cancellationToken = default)
    {
        var sessions = await _repository.GetCoordinatorSessionsAsync(cancellationToken);
        return sessions.Select(MapToSessionDto).ToList();
    }

    public async Task<bool> RevokeSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _repository.RevokeSessionAsync(sessionId, cancellationToken);
    }

    public async Task<CoordinatorSettingsDto> ResetCoordinatorSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _repository.ResetCoordinatorSettingsAsync(cancellationToken);
        return MapToCoordinatorDto(settings);
    }

    #endregion

    #region Partner Officer Settings

    public async Task<PartnerSettingsDto> GetPartnerSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetPartnerSettingsAsync(cancellationToken);
        return MapToPartnerDto(settings);
    }

    public async Task<PartnerProfileSettingsDto> UpdatePartnerProfileAsync(
        UpdatePartnerProfileSettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new PartnerProfileSettings
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Role = dto.Role ?? "Partner Officer"
        };

        var updated = await _repository.UpdatePartnerProfileAsync(entity, cancellationToken);
        return MapToPartnerProfileDto(updated);
    }

    public async Task<PartnerOrganisationSettingsDto> UpdatePartnerOrganisationAsync(
        UpdatePartnerOrganisationSettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new PartnerOrganisationSettings
        {
            Name = dto.Name,
            Registration = dto.Registration,
            Type = dto.Type,
            Location = dto.Location,
            Timezone = dto.Timezone,
            Language = dto.Language,
            Currency = dto.Currency
        };

        var updated = await _repository.UpdatePartnerOrganisationAsync(entity, cancellationToken);
        return MapToPartnerOrganisationDto(updated);
    }

    public async Task<PartnerSharingSettingsDto> UpdatePartnerSharingAsync(
        UpdatePartnerSharingSettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new PartnerSharingSettings
        {
            Referrals = dto.Referrals,
            Resources = dto.Resources,
            Activities = dto.Activities,
            Reports = dto.Reports,
            Documents = dto.Documents,
            PublicProfile = dto.PublicProfile
        };

        var updated = await _repository.UpdatePartnerSharingAsync(entity, cancellationToken);
        return MapToPartnerSharingDto(updated);
    }

    public async Task<PartnerNotificationSettingsDto> UpdatePartnerNotificationsAsync(
        UpdatePartnerNotificationSettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new PartnerNotificationSettings
        {
            Urgent = dto.Urgent,
            Referrals = dto.Referrals,
            Collaboration = dto.Collaboration,
            Resources = dto.Resources,
            Reports = dto.Reports,
            Team = dto.Team,
            Email = dto.Email,
            Push = dto.Push
        };

        var updated = await _repository.UpdatePartnerNotificationsAsync(entity, cancellationToken);
        return MapToPartnerNotificationsDto(updated);
    }

    public async Task<PartnerSecuritySettingsDto> UpdatePartnerSecurityAsync(
        UpdatePartnerSecuritySettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new PartnerSecuritySettings
        {
            TwoFactor = dto.TwoFactor,
            LoginAlerts = dto.LoginAlerts,
            SessionTimeout = dto.SessionTimeout
        };

        var updated = await _repository.UpdatePartnerSecurityAsync(entity, cancellationToken);
        return new PartnerSecuritySettingsDto
        {
            TwoFactor = updated.TwoFactor,
            LoginAlerts = updated.LoginAlerts,
            SessionTimeout = updated.SessionTimeout
        };
    }

    public async Task<ChangePasswordResponseDto> ChangePartnerPasswordAsync(
        ChangePasswordRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
        {
            return new ChangePasswordResponseDto
            {
                Success = false,
                Message = "New password and confirmation do not match."
            };
        }

        if (dto.NewPassword.Length < 6)
        {
            return new ChangePasswordResponseDto
            {
                Success = false,
                Message = "New password must contain at least 6 characters."
            };
        }

        bool verified = await _repository.VerifyPartnerPasswordAsync(dto.CurrentPassword, cancellationToken);
        if (!verified)
        {
            return new ChangePasswordResponseDto
            {
                Success = false,
                Message = "Current password is incorrect."
            };
        }

        bool changed = await _repository.ChangePartnerPasswordAsync(dto.NewPassword, cancellationToken);
        if (!changed)
        {
            return new ChangePasswordResponseDto
            {
                Success = false,
                Message = "Failed to update password. Please try again."
            };
        }

        return new ChangePasswordResponseDto
        {
            Success = true,
            Message = "Password has been updated successfully."
        };
    }

    public async Task<PartnerSettingsDto> ResetPartnerSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _repository.ResetPartnerSettingsAsync(cancellationToken);
        return MapToPartnerDto(settings);
    }

    #endregion

    #region Helper Mappers

    private static CoordinatorSettingsDto MapToCoordinatorDto(CoordinatorSettings entity)
    {
        return new CoordinatorSettingsDto
        {
            Profile = MapToProfileDto(entity.Profile),
            Operational = MapToOperationalDto(entity.Operational),
            Notifications = MapToNotificationsDto(entity.Notifications),
            Security = new CoordinatorSecuritySettingsDto
            {
                TwoFactorEnabled = entity.Security.TwoFactorEnabled,
                LoginAlerts = entity.Security.LoginAlerts,
                SessionTimeout = entity.Security.SessionTimeout,
                LastPasswordChangedAt = entity.Security.LastPasswordChangedAt
            },
            Sessions = entity.Sessions.Select(MapToSessionDto).ToList(),
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static CoordinatorProfileSettingsDto MapToProfileDto(CoordinatorProfileSettings entity)
    {
        return new CoordinatorProfileSettingsDto
        {
            FullName = entity.FullName,
            Email = entity.Email,
            Phone = entity.Phone,
            Area = entity.Area,
            Role = entity.Role,
            Initials = entity.Initials,
            BadgeNumber = entity.BadgeNumber
        };
    }

    private static CoordinatorOperationalSettingsDto MapToOperationalDto(CoordinatorOperationalSettings entity)
    {
        return new CoordinatorOperationalSettingsDto
        {
            AutoRefresh = entity.AutoRefresh,
            AiRecommendations = entity.AiRecommendations,
            CriticalAlerts = entity.CriticalAlerts,
            LiveMap = entity.LiveMap,
            DefaultPriority = entity.DefaultPriority,
            RefreshInterval = entity.RefreshInterval
        };
    }

    private static CoordinatorNotificationSettingsDto MapToNotificationsDto(CoordinatorNotificationSettings entity)
    {
        return new CoordinatorNotificationSettingsDto
        {
            Critical = entity.Critical,
            Ai = entity.Ai,
            Resources = entity.Resources,
            Shelters = entity.Shelters,
            Partners = entity.Partners,
            Reports = entity.Reports,
            Email = entity.Email,
            Browser = entity.Browser
        };
    }

    private static UserSessionDto MapToSessionDto(UserSession s)
    {
        return new UserSessionDto
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

    private static PartnerSettingsDto MapToPartnerDto(PartnerSettings entity)
    {
        return new PartnerSettingsDto
        {
            Profile = MapToPartnerProfileDto(entity.Profile),
            Organisation = MapToPartnerOrganisationDto(entity.Organisation),
            Sharing = MapToPartnerSharingDto(entity.Sharing),
            Notifications = MapToPartnerNotificationsDto(entity.Notifications),
            Security = new PartnerSecuritySettingsDto
            {
                TwoFactor = entity.Security.TwoFactorEnabled,
                LoginAlerts = entity.Security.LoginAlerts,
                SessionTimeout = entity.Security.SessionTimeout
            },
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static PartnerProfileSettingsDto MapToPartnerProfileDto(PartnerProfileSettings entity)
    {
        return new PartnerProfileSettingsDto
        {
            FullName = entity.FullName,
            Email = entity.Email,
            Phone = entity.Phone,
            Role = entity.Role
        };
    }

    private static PartnerOrganisationSettingsDto MapToPartnerOrganisationDto(PartnerOrganisationSettings entity)
    {
        return new PartnerOrganisationSettingsDto
        {
            Name = entity.Name,
            Registration = entity.Registration,
            Type = entity.Type,
            Location = entity.Location,
            Timezone = entity.Timezone,
            Language = entity.Language,
            Currency = entity.Currency
        };
    }

    private static PartnerSharingSettingsDto MapToPartnerSharingDto(PartnerSharingSettings entity)
    {
        return new PartnerSharingSettingsDto
        {
            Referrals = entity.Referrals,
            Resources = entity.Resources,
            Activities = entity.Activities,
            Reports = entity.Reports,
            Documents = entity.Documents,
            PublicProfile = entity.PublicProfile
        };
    }

    private static PartnerNotificationSettingsDto MapToPartnerNotificationsDto(PartnerNotificationSettings entity)
    {
        return new PartnerNotificationSettingsDto
        {
            Urgent = entity.Urgent,
            Referrals = entity.Referrals,
            Collaboration = entity.Collaboration,
            Resources = entity.Resources,
            Reports = entity.Reports,
            Team = entity.Team,
            Email = entity.Email,
            Push = entity.Push
        };
    }

    #endregion
}
