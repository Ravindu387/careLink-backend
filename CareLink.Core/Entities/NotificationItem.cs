namespace CareLink.Core.Entities;

public class NotificationItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "System"; // Critical, Requests, Inventory, System, Flags, Verification, Campaigns, Allocation, Expenditure, Reports
    public string Category
    {
        get => Type;
        set => Type = value;
    }
    public string Icon { get; set; } = "resident"; // request, warning, inventory, medical, maintenance, resident, alert
    public string Time { get; set; } = string.Empty;
    public bool Unread { get; set; } = true;
    public bool Urgent { get; set; } = false;
    public string Reference { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? ActionLabel { get; set; }
    public string Role { get; set; } = "ShelterAdmin"; // ShelterAdmin, FundingOfficer, All, coordinator, partner
    public string? TargetRole
    {
        get => Role;
        set => Role = value ?? "all";
    }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTimeOffset? ReadAt { get; set; }
}
