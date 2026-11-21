namespace CareLink.Core.Entities;

public class UserSettings
{
    public string Id { get; set; } = "default";
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = "Shelter Administrator";

    // Shelter info
    public string ShelterName { get; set; } = string.Empty;
    public string ShelterLocation { get; set; } = string.Empty;
    public string ShelterCapacity { get; set; } = string.Empty;
    public string ShelterManager { get; set; } = string.Empty;
    public string ShelterAddress { get; set; } = string.Empty;

    // Security
    public bool TwoFactor { get; set; } = true;
    public bool LoginAlerts { get; set; } = true;

    // Notification preferences
    public bool NotifInApp { get; set; } = true;
    public bool NotifEmail { get; set; } = true;
    public bool NotifSms { get; set; } = false;
    public bool NotifCritical { get; set; } = true;
    public bool NotifRequests { get; set; } = true;
    public bool NotifInventory { get; set; } = true;
    public bool NotifSystem { get; set; } = false;
}
