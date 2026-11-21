namespace CareLink.Core.Entities;

public class NotificationPreferences
{
    // Coordinator categories
    public bool Critical { get; set; } = true;
    public bool Ai { get; set; } = true;
    public bool Resources { get; set; } = true;
    public bool Shelter { get; set; } = true;
    public bool Partner { get; set; } = true;
    public bool Reports { get; set; } = true;

    // Partner Officer categories
    public bool Urgent { get; set; } = true;
    public bool Referrals { get; set; } = true;
    public bool Collaboration { get; set; } = true;
    public bool Reporting { get; set; } = true;
    public bool Team { get; set; } = true;
    public bool Email { get; set; } = false;
    public bool Push { get; set; } = true;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
