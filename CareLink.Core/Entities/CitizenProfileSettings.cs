namespace CareLink.Core.Entities;

public class SecuritySessionItem
{
    public string Device { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string LastActive { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}

public class CitizenProfileSettings
{
    public string Id { get; set; } = "CPS-001";
    public string CitizenReference { get; set; } = "CIT-10482";
    public string CitizenName { get; set; } = "Samira Khan";
    public string PreferredName { get; set; } = "Samira";
    public string DateOfBirth { get; set; } = "1990-12-04";
    public string NationalId { get; set; } = "199084501234";
    public string Gender { get; set; } = "Female";

    // Contact & Location
    public string Email { get; set; } = "samira.khan@email.com";
    public string Phone { get; set; } = "+94 77 412 8088";
    public string Address { get; set; } = "North Ward, Greater Riverton";
    public string District { get; set; } = "North Ward";
    public string City { get; set; } = "Greater Riverton";
    public string PostalCode { get; set; } = "10230";
    public string EmergencyContactName { get; set; } = "Rashid Khan";
    public string EmergencyContactPhone { get; set; } = "+94 77 455 0182";
    public string EmergencyContactRelation { get; set; } = "Brother";

    // Language & Accessibility
    public string Language { get; set; } = "English";
    public string Accessibility { get; set; } = "Standard";
    public bool HighContrast { get; set; } = false;
    public bool LargeText { get; set; } = false;
    public bool ScreenReaderSupport { get; set; } = false;
    public bool AudioAssistance { get; set; } = false;

    // Notification Preferences
    public bool EmergencyAlerts { get; set; } = true;
    public bool RequestStatusUpdates { get; set; } = true;
    public bool SafetyNotifications { get; set; } = true;
    public bool MessagesFromSupportTeams { get; set; } = true;
    public bool ShelterAvailabilityUpdates { get; set; } = true;
    public bool SmsAlerts { get; set; } = true;
    public bool EmailAlerts { get; set; } = true;

    // Privacy & Data
    public bool ShareLocationDuringDisaster { get; set; } = true;
    public bool ShareMedicalWithRescuers { get; set; } = true;
    public bool AllowVolunteersContact { get; set; } = true;
    public bool DataRetentionConsent { get; set; } = true;
    public string PersonalInformationPolicy { get; set; } = "Your personal information is protected and only shared with authorised support teams when necessary.";
    public string DataSharingPolicy { get; set; } = "Control how your information is used when requesting assistance.";

    // Security
    public string PasswordHash { get; set; } = "Pass@1234"; // Default simulated credential
    public bool TwoFactorEnabled { get; set; } = true;
    public DateTime? LastPasswordChange { get; set; } = DateTime.UtcNow.AddDays(-30);
    public DateTime? LastLoginAt { get; set; } = DateTime.UtcNow;
    public List<SecuritySessionItem> ActiveSessions { get; set; } = new()
    {
        new SecuritySessionItem { Device = "Chrome on Windows (Current)", Location = "Colombo, Sri Lanka", LastActive = "Just now", IsCurrent = true },
        new SecuritySessionItem { Device = "CareLink Mobile App on iOS", Location = "Riverton District", LastActive = "3 hours ago", IsCurrent = false }
    };

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddMonths(-6);
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
}
