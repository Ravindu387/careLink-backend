namespace CareLink.Core.Entities;

public class NotificationItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "System"; // Critical, Requests, Inventory, System
    public string Icon { get; set; } = "resident"; // request, warning, inventory, medical, maintenance, resident
    public bool Unread { get; set; } = true;
    public string Details { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Role { get; set; } = "ShelterAdmin"; // ShelterAdmin, FundingOfficer, All
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
