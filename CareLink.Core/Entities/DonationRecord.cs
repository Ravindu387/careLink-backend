namespace CareLink.Core.Entities;

public class DonationRecord
{
    public string Id { get; set; } = string.Empty; // e.g. DON-88421
    public string Donor { get; set; } = string.Empty;
    public string Type { get; set; } = "Monetary"; // Monetary | Material
    public string Amount { get; set; } = string.Empty; // e.g. "$125,000" or "4,800 food packs"
    public decimal? NumericAmount { get; set; }
    public string Campaign { get; set; } = string.Empty;
    public string Received { get; set; } = string.Empty; // e.g. "22 Jun"
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    public string Verification { get; set; } = "Pending"; // Verified | Pending | Flagged
    public string Allocation { get; set; } = "Unallocated"; // Allocated | Partially Allocated | Unallocated | On Hold
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
