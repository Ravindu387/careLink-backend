namespace CareLink.Application.DTOs.Settings;

public class SystemSettingsDto
{
    // General
    public string PlatformName { get; set; } = "CareLink Humanitarian Aid Platform";
    public string Language { get; set; } = "English";
    public string Timezone { get; set; } = "UTC+05:30 · Colombo";
    public string SupportEmail { get; set; } = "support@carelink.org";
    public string MaintenanceNotice { get; set; } = "No active maintenance notice";

    // Security & Authentication
    public bool TwoFactor { get; set; } = true;
    public bool CompromisedPasswords { get; set; } = true;
    public bool SuspiciousSession { get; set; } = true;
    public bool AdminApproval { get; set; } = true;
    public string SessionTimeout { get; set; } = "20";
    public string FailedAttempts { get; set; } = "5";
    public string PasswordLength { get; set; } = "12";
    public string ResetExpiry { get; set; } = "30";
    public bool PartnerSSO { get; set; } = true;

    // Notifications
    public bool SecurityAlerts { get; set; } = true;
    public bool IntegrationAlerts { get; set; } = true;
    public bool RoleRequests { get; set; } = true;
    public bool FailedJobs { get; set; } = true;
    public bool DailySummary { get; set; } = true;

    // Data & Retention
    public string AuditRetention { get; set; } = "7 years";
    public string InactiveRetention { get; set; } = "24 months";
    public string ExportExpiry { get; set; } = "24 hours";
    public string LogAggregation { get; set; } = "90 days";

    // Feature Controls
    public bool AiDecision { get; set; } = true;
    public bool CitizenRegistration { get; set; } = true;
    public bool PartnerSharing { get; set; } = true;
    public bool SmsDelivery { get; set; } = true;
    public bool DemandForecasts { get; set; } = false;

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    public string LastUpdatedBy { get; set; } = "SystemAdmin";
}

public class UpdateSystemSettingsDto
{
    public Dictionary<string, object> Settings { get; set; } = new();
}
