namespace CareLink.Backend.Models;

public class VolunteerProfileDto
{
    public string FullName { get; set; } = "Nadia Okafor";
    public string Email { get; set; } = "nadia@reliefos.org";
    public string Phone { get; set; } = "+94 77 412 8041";
    public string Area { get; set; } = "Greater Riverton";
    public string? Photo { get; set; }
}

public class AIMatchingRulesDto
{
    public string MinimumScore { get; set; } = "70";
    public string TravelDistance { get; set; } = "25";
    public bool Availability { get; set; } = true;
    public bool VerifiedSkills { get; set; } = true;
    public bool Experience { get; set; } = true;
    public bool Training { get; set; } = true;
    public bool PreventConflicts { get; set; } = true;
    public bool HumanApproval { get; set; } = true;
}

public class VolunteerSecuritySettingsDto
{
    public bool TwoFactor { get; set; } = true;
}

public class VolunteerActiveSessionDto
{
    public string Id { get; set; } = "sess-01";
    public string Device { get; set; } = "Windows";
    public string Browser { get; set; } = "Chrome";
    public string Location { get; set; } = "Riverton Operations Center";
    public string LastActive { get; set; } = "Active now";
    public bool IsCurrent { get; set; } = true;
}

public class VolunteerSettingsDto
{
    public VolunteerProfileDto Profile { get; set; } = new();
    public AIMatchingRulesDto MatchingRules { get; set; } = new();
    public VolunteerNotificationPreferencesDto NotificationSettings { get; set; } = new();
    public VolunteerSecuritySettingsDto Security { get; set; } = new();
    public List<VolunteerActiveSessionDto> ActiveSessions { get; set; } = [];
}

public class VolunteerChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
