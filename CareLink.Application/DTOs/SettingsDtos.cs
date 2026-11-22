using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

#region Emergency Coordinator Settings DTOs

public class CoordinatorSettingsDto
{
    public CoordinatorProfileSettingsDto Profile { get; set; } = new();
    public CoordinatorOperationalSettingsDto Operational { get; set; } = new();
    public CoordinatorNotificationSettingsDto Notifications { get; set; } = new();
    public CoordinatorSecuritySettingsDto Security { get; set; } = new();
    public List<UserSessionDto> Sessions { get; set; } = new();
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CoordinatorProfileSettingsDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string BadgeNumber { get; set; } = string.Empty;
}

public class UpdateCoordinatorProfileSettingsDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    [Required]
    public string Area { get; set; } = string.Empty;

    public string? Role { get; set; }
}

public class CoordinatorOperationalSettingsDto
{
    public bool AutoRefresh { get; set; }
    public bool AiRecommendations { get; set; }
    public bool CriticalAlerts { get; set; }
    public bool LiveMap { get; set; }
    public string DefaultPriority { get; set; } = "High";
    public string RefreshInterval { get; set; } = "30 seconds";
}

public class UpdateCoordinatorOperationalSettingsDto
{
    public bool AutoRefresh { get; set; }
    public bool AiRecommendations { get; set; }
    public bool CriticalAlerts { get; set; }
    public bool LiveMap { get; set; }

    [Required]
    public string DefaultPriority { get; set; } = "High";

    [Required]
    public string RefreshInterval { get; set; } = "30 seconds";
}

public class CoordinatorNotificationSettingsDto
{
    public bool Critical { get; set; }
    public bool Ai { get; set; }
    public bool Resources { get; set; }
    public bool Shelters { get; set; }
    public bool Partners { get; set; }
    public bool Reports { get; set; }
    public bool Email { get; set; }
    public bool Browser { get; set; }
}

public class UpdateCoordinatorNotificationSettingsDto
{
    public bool Critical { get; set; }
    public bool Ai { get; set; }
    public bool Resources { get; set; }
    public bool Shelters { get; set; }
    public bool Partners { get; set; }
    public bool Reports { get; set; }
    public bool Email { get; set; }
    public bool Browser { get; set; }
}

public class CoordinatorSecuritySettingsDto
{
    public bool TwoFactorEnabled { get; set; }
    public bool LoginAlerts { get; set; }
    public string SessionTimeout { get; set; } = "30 minutes";
    public DateTimeOffset LastPasswordChangedAt { get; set; }
}

public class UserSessionDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DeviceType { get; set; } = "desktop";
    public string Time { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public bool Current { get; set; }
    public DateTimeOffset LastActiveAt { get; set; }
}

public class ChangePasswordRequestDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "New password and confirmation do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ChangePasswordResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion

#region Partner Settings DTOs

public class PartnerSettingsDto
{
    public PartnerProfileSettingsDto Profile { get; set; } = new();
    public PartnerOrganisationSettingsDto Organisation { get; set; } = new();
    public PartnerSharingSettingsDto Sharing { get; set; } = new();
    public PartnerNotificationSettingsDto Notifications { get; set; } = new();
    public PartnerSecuritySettingsDto Security { get; set; } = new();
    public DateTimeOffset UpdatedAt { get; set; }
}

public class PartnerProfileSettingsDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class UpdatePartnerProfileSettingsDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Role { get; set; }
}

public class PartnerOrganisationSettingsDto
{
    public string Name { get; set; } = string.Empty;
    public string Registration { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
}

public class UpdatePartnerOrganisationSettingsDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Registration { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Timezone { get; set; } = "Asia/Colombo";
    public string Language { get; set; } = "English";
    public string Currency { get; set; } = "LKR";
}

public class PartnerSharingSettingsDto
{
    public bool Referrals { get; set; }
    public bool Resources { get; set; }
    public bool Activities { get; set; }
    public bool Reports { get; set; }
    public bool Documents { get; set; }
    public bool PublicProfile { get; set; }
}

public class UpdatePartnerSharingSettingsDto
{
    public bool Referrals { get; set; }
    public bool Resources { get; set; }
    public bool Activities { get; set; }
    public bool Reports { get; set; }
    public bool Documents { get; set; }
    public bool PublicProfile { get; set; }
}

public class PartnerNotificationSettingsDto
{
    public bool Urgent { get; set; }
    public bool Referrals { get; set; }
    public bool Collaboration { get; set; }
    public bool Resources { get; set; }
    public bool Reports { get; set; }
    public bool Team { get; set; }
    public bool Email { get; set; }
    public bool Push { get; set; }
}

public class UpdatePartnerNotificationSettingsDto
{
    public bool Urgent { get; set; }
    public bool Referrals { get; set; }
    public bool Collaboration { get; set; }
    public bool Resources { get; set; }
    public bool Reports { get; set; }
    public bool Team { get; set; }
    public bool Email { get; set; }
    public bool Push { get; set; }
}

public class PartnerSecuritySettingsDto
{
    public bool TwoFactor { get; set; }
    public bool LoginAlerts { get; set; }
    public string SessionTimeout { get; set; } = "30 minutes";
}

public class UpdatePartnerSecuritySettingsDto
{
    public bool TwoFactor { get; set; }
    public bool LoginAlerts { get; set; }
    public string SessionTimeout { get; set; } = "30 minutes";
}

#endregion
