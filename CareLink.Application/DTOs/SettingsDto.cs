namespace CareLink.Application.DTOs;

public class ProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class ShelterSettingsDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Capacity { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class SecuritySettingsDto
{
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public bool TwoFactor { get; set; }
    public bool LoginAlerts { get; set; }
}

public class NotificationSettingsDto
{
    public bool InApp { get; set; }
    public bool Email { get; set; }
    public bool Sms { get; set; }
    public bool Critical { get; set; }
    public bool Requests { get; set; }
    public bool Inventory { get; set; }
    public bool System { get; set; }
}
