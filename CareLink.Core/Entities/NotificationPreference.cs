namespace CareLink.Core.Entities;

public class NotificationPreference
{
    public int Id { get; set; }
    public string Role { get; set; } = "ShelterAdmin";
    public bool InApp { get; set; } = true;
    public bool Email { get; set; } = true;
    public bool Sms { get; set; } = false;
    public bool Critical { get; set; } = true;
    public bool Requests { get; set; } = true;
    public bool Inventory { get; set; } = true;
    public bool System { get; set; } = false;
    public bool Verification { get; set; } = true;
    public bool Campaigns { get; set; } = true;
    public bool Reports { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
