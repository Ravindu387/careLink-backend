namespace CareLink.Application.DTOs;

public class SecuritySessionDto
{
    public string Device { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string LastActive { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}

public class CitizenProfileSettingsDto
{
    public string Id { get; set; } = string.Empty;
    public string CitizenReference { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PreferredName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;

    // Contact & Location
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string EmergencyContactRelation { get; set; } = string.Empty;

    // Language & Accessibility
    public string Language { get; set; } = "English";
    public string Accessibility { get; set; } = "Standard";
    public bool HighContrast { get; set; }
    public bool LargeText { get; set; }
    public bool ScreenReaderSupport { get; set; }
    public bool AudioAssistance { get; set; }

    // Notifications
    public bool EmergencyAlerts { get; set; }
    public bool RequestStatusUpdates { get; set; }
    public bool SafetyNotifications { get; set; }
    public bool MessagesFromSupportTeams { get; set; }
    public bool ShelterAvailabilityUpdates { get; set; }
    public bool SmsAlerts { get; set; }
    public bool EmailAlerts { get; set; }

    // Privacy & Data
    public bool ShareLocationDuringDisaster { get; set; }
    public bool ShareMedicalWithRescuers { get; set; }
    public bool AllowVolunteersContact { get; set; }
    public bool DataRetentionConsent { get; set; }
    public string PersonalInformationPolicy { get; set; } = string.Empty;
    public string DataSharingPolicy { get; set; } = string.Empty;

    // Security
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LastPasswordChange { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<SecuritySessionDto> ActiveSessions { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdatePersonalDetailsRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? PreferredName { get; set; }
    public string? DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    public string? Gender { get; set; }
}

public class UpdateContactLocationRequest
{
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
}

public class UpdateLanguageAccessibilityRequest
{
    public string Language { get; set; } = "English";
    public string Accessibility { get; set; } = "Standard";
    public bool HighContrast { get; set; }
    public bool LargeText { get; set; }
    public bool ScreenReaderSupport { get; set; }
    public bool AudioAssistance { get; set; }
}

public class UpdateNotificationPreferencesRequest
{
    public bool EmergencyAlerts { get; set; }
    public bool RequestStatusUpdates { get; set; }
    public bool SafetyNotifications { get; set; }
    public bool MessagesFromSupportTeams { get; set; }
    public bool ShelterAvailabilityUpdates { get; set; }
    public bool SmsAlerts { get; set; }
    public bool EmailAlerts { get; set; }
}

public class UpdatePrivacyDataRequest
{
    public bool ShareLocationDuringDisaster { get; set; }
    public bool ShareMedicalWithRescuers { get; set; }
    public bool AllowVolunteersContact { get; set; }
    public bool DataRetentionConsent { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ToggleTwoFactorRequest
{
    public bool Enabled { get; set; }
}

public class UpdateFullProfileRequest
{
    public string? FullName { get; set; }
    public string? PreferredName { get; set; }
    public string? DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    public string? Gender { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? Language { get; set; }
    public string? Accessibility { get; set; }
    public bool? HighContrast { get; set; }
    public bool? LargeText { get; set; }
    public bool? ScreenReaderSupport { get; set; }
    public bool? AudioAssistance { get; set; }
    public bool? EmergencyAlerts { get; set; }
    public bool? RequestStatusUpdates { get; set; }
    public bool? SafetyNotifications { get; set; }
    public bool? MessagesFromSupportTeams { get; set; }
    public bool? ShelterAvailabilityUpdates { get; set; }
    public bool? SmsAlerts { get; set; }
    public bool? EmailAlerts { get; set; }
    public bool? ShareLocationDuringDisaster { get; set; }
    public bool? ShareMedicalWithRescuers { get; set; }
    public bool? AllowVolunteersContact { get; set; }
    public bool? DataRetentionConsent { get; set; }
}
