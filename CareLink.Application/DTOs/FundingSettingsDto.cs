using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class FundingSettingsDto
{
    // Profile
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Contact { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    // Funding Controls
    [Range(0, double.MaxValue)]
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
}

public class ChangeFundingPasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
