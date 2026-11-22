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
    private readonly CareLink.Core.Interfaces.ISettingsRepository _repository;
    private readonly CareLink.Application.Interfaces.ISettingsRepository? _resourceManagerRepository;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(
        CareLink.Core.Interfaces.ISettingsRepository repository,
        ILogger<SettingsService> logger,
        CareLink.Application.Interfaces.ISettingsRepository? resourceManagerRepository = null)
    {
        _repository = repository;
        _logger = logger;
        _resourceManagerRepository = resourceManagerRepository;
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

    #region Shelter Admin Settings (Prabhani)

    private async Task<UserSettings> GetOrCreateAsync()
    {
        var settings = await _repository.GetAsync();
        if (settings != null) return settings;

        // Seed defaults if not found
        var defaults = new UserSettings
        {
            Id = "default",
            FullName = "Sanjana Amarasinghe",
            Email = "sanjana@carelink.org",
            Phone = "+94 77 321 8490",
            Role = "Shelter Administrator",
            ShelterName = "Riverton Central School",
            ShelterLocation = "Central District",
            ShelterCapacity = "660",
            ShelterManager = "Sanjana Amarasinghe",
            ShelterAddress = "Riverton, Greater Riverton",
            TwoFactor = true,
            LoginAlerts = true,
            NotifInApp = true,
            NotifEmail = true,
            NotifSms = false,
            NotifCritical = true,
            NotifRequests = true,
            NotifInventory = true,
            NotifSystem = false,
        };
        return await _repository.UpsertAsync(defaults);
    }

    public async Task<ProfileDto> GetProfileAsync()
    {
        var s = await GetOrCreateAsync();
        return new ProfileDto
        {
            FullName = s.FullName,
            Email = s.Email,
            Phone = s.Phone,
            Role = s.Role,
        };
    }

    public async Task<ProfileDto> UpdateProfileAsync(ProfileDto dto)
    {
        var s = await GetOrCreateAsync();
        s.FullName = dto.FullName;
        s.Email = dto.Email;
        s.Phone = dto.Phone;
        // Role is read-only — do not update
        await _repository.UpsertAsync(s);
        return new ProfileDto
        {
            FullName = s.FullName,
            Email = s.Email,
            Phone = s.Phone,
            Role = s.Role,
        };
    }

    public async Task<ShelterSettingsDto> GetShelterSettingsAsync()
    {
        var s = await GetOrCreateAsync();
        return new ShelterSettingsDto
        {
            Name = s.ShelterName,
            Location = s.ShelterLocation,
            Capacity = s.ShelterCapacity,
            Manager = s.ShelterManager,
            Address = s.ShelterAddress,
        };
    }

    public async Task<ShelterSettingsDto> UpdateShelterSettingsAsync(ShelterSettingsDto dto)
    {
        var s = await GetOrCreateAsync();
        s.ShelterName = dto.Name;
        s.ShelterLocation = dto.Location;
        s.ShelterCapacity = dto.Capacity;
        s.ShelterManager = dto.Manager;
        s.ShelterAddress = dto.Address;
        await _repository.UpsertAsync(s);
        return new ShelterSettingsDto
        {
            Name = s.ShelterName,
            Location = s.ShelterLocation,
            Capacity = s.ShelterCapacity,
            Manager = s.ShelterManager,
            Address = s.ShelterAddress,
        };
    }

    public async Task<SecuritySettingsDto> GetSecuritySettingsAsync()
    {
        var s = await GetOrCreateAsync();
        return new SecuritySettingsDto
        {
            TwoFactor = s.TwoFactor,
            LoginAlerts = s.LoginAlerts,
        };
    }

    public async Task<bool> UpdateSecuritySettingsAsync(SecuritySettingsDto dto)
    {
        var s = await GetOrCreateAsync();
        s.TwoFactor = dto.TwoFactor;
        s.LoginAlerts = dto.LoginAlerts;
        await _repository.UpsertAsync(s);
        return true;
    }

    public async Task<NotificationSettingsDto> GetNotificationSettingsAsync()
    {
        var s = await GetOrCreateAsync();
        return new NotificationSettingsDto
        {
            InApp = s.NotifInApp,
            Email = s.NotifEmail,
            Sms = s.NotifSms,
            Critical = s.NotifCritical,
            Requests = s.NotifRequests,
            Inventory = s.NotifInventory,
            System = s.NotifSystem,
        };
    }

    public async Task<NotificationSettingsDto> UpdateNotificationSettingsAsync(NotificationSettingsDto dto)
    {
        var s = await GetOrCreateAsync();
        s.NotifInApp = dto.InApp;
        s.NotifEmail = dto.Email;
        s.NotifSms = dto.Sms;
        s.NotifCritical = dto.Critical;
        s.NotifRequests = dto.Requests;
        s.NotifInventory = dto.Inventory;
        s.NotifSystem = dto.System;
        await _repository.UpsertAsync(s);
        return new NotificationSettingsDto
        {
            InApp = s.NotifInApp,
            Email = s.NotifEmail,
            Sms = s.NotifSms,
            Critical = s.NotifCritical,
            Requests = s.NotifRequests,
            Inventory = s.NotifInventory,
            System = s.NotifSystem,
        };
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

    #region Resource Manager Settings (Oshadi)

    private CareLink.Application.Interfaces.ISettingsRepository GetResourceManagerRepo() =>
        _resourceManagerRepository ?? throw new InvalidOperationException("ResourceManager ISettingsRepository is not registered.");

    public async Task<ApiResponse<ResourceManagerSettingsDto>> GetResourceManagerSettingsAsync()
    {
        var settings = await GetResourceManagerRepo().GetSettingsAsync();
        var dto = MapToDto(settings);
        return ApiResponse<ResourceManagerSettingsDto>.SuccessResponse(dto, "Settings retrieved successfully");
    }

    public async Task<ApiResponse<ResourceManagerSettingsDto>> UpdateSettingsAsync(UpdateSettingsRequest request)
    {
        var repo = GetResourceManagerRepo();
        var current = await repo.GetSettingsAsync();

        if (request.Profile != null)
        {
            current.Profile.FullName = request.Profile.FullName ?? current.Profile.FullName;
            current.Profile.Email = request.Profile.Email ?? current.Profile.Email;
            current.Profile.Phone = request.Profile.Phone ?? current.Profile.Phone;
            current.Profile.Warehouse = request.Profile.Warehouse ?? current.Profile.Warehouse;
        }

        if (request.Inventory != null)
        {
            current.Inventory.LowStock = request.Inventory.LowStock;
            current.Inventory.CriticalStock = request.Inventory.CriticalStock;
            current.Inventory.ExpiryDays = request.Inventory.ExpiryDays;
            current.Inventory.AutoReorder = request.Inventory.AutoReorder;
            current.Inventory.TrackExpiry = request.Inventory.TrackExpiry;
            current.Inventory.AllowNegative = request.Inventory.AllowNegative;
        }

        if (request.Dispatch != null)
        {
            current.Dispatch.DefaultVehicle = request.Dispatch.DefaultVehicle;
            current.Dispatch.Approval = request.Dispatch.Approval;
            current.Dispatch.Proof = request.Dispatch.Proof;
            current.Dispatch.Tracking = request.Dispatch.Tracking;
            current.Dispatch.DelayAlert = request.Dispatch.DelayAlert;
        }

        if (request.Notifications != null)
        {
            current.Notifications.Email = request.Notifications.Email;
            current.Notifications.System = request.Notifications.System;
            current.Notifications.Critical = request.Notifications.Critical;
            current.Notifications.LowStock = request.Notifications.LowStock;
            current.Notifications.Dispatch = request.Notifications.Dispatch;
            current.Notifications.Weekly = request.Notifications.Weekly;
        }

        if (request.Security != null)
        {
            current.Security.TwoFactor = request.Security.TwoFactor;
            current.Security.SessionTimeout = request.Security.SessionTimeout;
            current.Security.LoginAlerts = request.Security.LoginAlerts;
        }

        var updated = await repo.UpdateSettingsAsync(current);
        var dto = MapToDto(updated);
        return ApiResponse<ResourceManagerSettingsDto>.SuccessResponse(dto, "Settings updated successfully");
    }

    public async Task<ApiResponse<ResourceManagerSettingsDto>> ResetSettingsAsync()
    {
        var reset = await GetResourceManagerRepo().ResetSettingsAsync();
        var dto = MapToDto(reset);
        return ApiResponse<ResourceManagerSettingsDto>.SuccessResponse(dto, "Settings reset to defaults successfully");
    }

    public async Task<ApiResponse<ProfileSettingsDto>> UpdateProfileAsync(ProfileSettingsDto dto)
    {
        var repo = GetResourceManagerRepo();
        var current = await repo.GetSettingsAsync();
        current.Profile = new ProfileSettings
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Warehouse = dto.Warehouse
        };

        var updated = await repo.UpdateSettingsAsync(current);
        return ApiResponse<ProfileSettingsDto>.SuccessResponse(MapProfileToDto(updated.Profile), "Profile updated successfully");
    }

    public async Task<ApiResponse<InventoryRulesSettingsDto>> UpdateInventoryRulesAsync(InventoryRulesSettingsDto dto)
    {
        var repo = GetResourceManagerRepo();
        var current = await repo.GetSettingsAsync();
        current.Inventory = new InventoryRulesSettings
        {
            LowStock = dto.LowStock,
            CriticalStock = dto.CriticalStock,
            ExpiryDays = dto.ExpiryDays,
            AutoReorder = dto.AutoReorder,
            TrackExpiry = dto.TrackExpiry,
            AllowNegative = dto.AllowNegative
        };

        var updated = await repo.UpdateSettingsAsync(current);
        return ApiResponse<InventoryRulesSettingsDto>.SuccessResponse(MapInventoryToDto(updated.Inventory), "Inventory rules updated successfully");
    }

    public async Task<ApiResponse<DispatchPreferencesSettingsDto>> UpdateDispatchPreferencesAsync(DispatchPreferencesSettingsDto dto)
    {
        var repo = GetResourceManagerRepo();
        var current = await repo.GetSettingsAsync();
        current.Dispatch = new DispatchPreferencesSettings
        {
            DefaultVehicle = dto.DefaultVehicle,
            Approval = dto.Approval,
            Proof = dto.Proof,
            Tracking = dto.Tracking,
            DelayAlert = dto.DelayAlert
        };

        var updated = await repo.UpdateSettingsAsync(current);
        return ApiResponse<DispatchPreferencesSettingsDto>.SuccessResponse(MapDispatchToDto(updated.Dispatch), "Dispatch preferences updated successfully");
    }

    public async Task<ApiResponse<NotificationPreferencesSettingsDto>> UpdateNotificationPreferencesAsync(NotificationPreferencesSettingsDto dto)
    {
        var repo = GetResourceManagerRepo();
        var current = await repo.GetSettingsAsync();
        current.Notifications = new NotificationPreferencesSettings
        {
            Email = dto.Email,
            System = dto.System,
            Critical = dto.Critical,
            LowStock = dto.LowStock,
            Dispatch = dto.Dispatch,
            Weekly = dto.Weekly
        };

        var updated = await repo.UpdateSettingsAsync(current);
        return ApiResponse<NotificationPreferencesSettingsDto>.SuccessResponse(MapNotificationsToDto(updated.Notifications), "Notification preferences updated successfully");
    }

    public async Task<ApiResponse<SecuritySettingsDto>> UpdateResourceManagerSecuritySettingsAsync(SecuritySettingsDto dto)
    {
        var repo = GetResourceManagerRepo();
        var current = await repo.GetSettingsAsync();
        current.Security = new SecuritySettings
        {
            TwoFactor = dto.TwoFactor,
            SessionTimeout = dto.SessionTimeout,
            LoginAlerts = dto.LoginAlerts
        };

        var updated = await repo.UpdateSettingsAsync(current);
        return ApiResponse<SecuritySettingsDto>.SuccessResponse(MapSecurityToDto(updated.Security), "Security settings updated successfully");
    }

    private static ResourceManagerSettingsDto MapToDto(ResourceManagerSettings entity)
    {
        return new ResourceManagerSettingsDto
        {
            Profile = MapProfileToDto(entity.Profile),
            Inventory = MapInventoryToDto(entity.Inventory),
            Dispatch = MapDispatchToDto(entity.Dispatch),
            Notifications = MapNotificationsToDto(entity.Notifications),
            Security = MapSecurityToDto(entity.Security),
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static ProfileSettingsDto MapProfileToDto(ProfileSettings profile)
    {
        return new ProfileSettingsDto
        {
            FullName = profile.FullName,
            Email = profile.Email,
            Phone = profile.Phone,
            Warehouse = profile.Warehouse
        };
    }

    private static InventoryRulesSettingsDto MapInventoryToDto(InventoryRulesSettings inventory)
    {
        return new InventoryRulesSettingsDto
        {
            LowStock = inventory.LowStock,
            CriticalStock = inventory.CriticalStock,
            ExpiryDays = inventory.ExpiryDays,
            AutoReorder = inventory.AutoReorder,
            TrackExpiry = inventory.TrackExpiry,
            AllowNegative = inventory.AllowNegative
        };
    }

    private static DispatchPreferencesSettingsDto MapDispatchToDto(DispatchPreferencesSettings dispatch)
    {
        return new DispatchPreferencesSettingsDto
        {
            DefaultVehicle = dispatch.DefaultVehicle,
            Approval = dispatch.Approval,
            Proof = dispatch.Proof,
            Tracking = dispatch.Tracking,
            DelayAlert = dispatch.DelayAlert
        };
    }

    private static NotificationPreferencesSettingsDto MapNotificationsToDto(NotificationPreferencesSettings notifications)
    {
        return new NotificationPreferencesSettingsDto
        {
            Email = notifications.Email,
            System = notifications.System,
            Critical = notifications.Critical,
            LowStock = notifications.LowStock,
            Dispatch = notifications.Dispatch,
            Weekly = notifications.Weekly
        };
    }

    private static SecuritySettingsDto MapSecurityToDto(SecuritySettings security)
    {
        return new SecuritySettingsDto
        {
            TwoFactor = security.TwoFactor,
            SessionTimeout = security.SessionTimeout,
            LoginAlerts = security.LoginAlerts
        };
    }

    #endregion
}
