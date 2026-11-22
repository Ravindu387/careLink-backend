namespace CareLink.Core.Entities;

public class FundingSettings
{
    public string Id { get; set; } = "funding-officer-default";

    // Profile
    public string FullName { get; set; } = "Maria Kone";
    public string Email { get; set; } = "maria@reliefos.org";
    public string Contact { get; set; } = "+94 77 412 8041";
    public string Currency { get; set; } = "USD";

    // Funding Controls
    public decimal ApprovalLimit { get; set; } = 50000m;
    public bool RequireApproval { get; set; } = true;
    public bool DualApproval { get; set; } = true;
    public bool AutoVerification { get; set; } = false;
    public bool RestrictedAllocation { get; set; } = true;

    // Delivery channels
    public bool EmailNotifications { get; set; } = true;
    public bool InAppNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;

    // Notification categories
    public bool NotifyDonations { get; set; } = true;
    public bool NotifyApprovals { get; set; } = true;
    public bool NotifyCampaigns { get; set; } = true;
    public bool NotifySecurity { get; set; } = true;

    // Security
    public bool TwoFactorEnabled { get; set; } = true;
    public string ActiveDevice { get; set; } = "Windows · Chrome";
    public string LastPasswordUpdate { get; set; } = "Recently";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
