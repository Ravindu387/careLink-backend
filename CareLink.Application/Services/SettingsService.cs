using CareLink.Application.Common;
using CareLink.Application.DTOs.Settings;
using CareLink.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CareLink.Application.Services;

public class SettingsService : ISettingsService
{
    private SystemSettingsDto _settings = new();
    private readonly object _lock = new();
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
    }

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
                        if (bool.TryParse(val.ToString(), out var bsd)) _settings.SmsDelivery = bsd;
                        break;
                    case "demandforecasts":
                        if (bool.TryParse(val.ToString(), out var bdf)) _settings.DemandForecasts = bdf;
                        break;
                }
            }

            _settings.LastUpdatedAt = DateTime.UtcNow;
            _settings.LastUpdatedBy = actor;

            _logger.LogInformation("System configuration updated by {Actor}", actor);
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
            _logger.LogInformation("System settings reset to default by {Actor}", actor);
            return Task.FromResult(ApiResponse<SystemSettingsDto>.SuccessResult(_settings, "Settings reset to default."));
        }
    }
}
