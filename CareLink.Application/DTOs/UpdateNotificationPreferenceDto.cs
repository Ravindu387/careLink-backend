namespace CareLink.Application.DTOs;

public class UpdateNotificationPreferenceDto
{
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
}
