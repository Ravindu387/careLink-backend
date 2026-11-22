namespace CareLink.Application.DTOs;

public class DonationDto
{
    public string Id { get; set; } = string.Empty;
    public string Donor { get; set; } = string.Empty;
    public string Type { get; set; } = "Monetary"; // Monetary | Material
    public string Amount { get; set; } = string.Empty; // e.g. "$125,000" or "4,800 food packs"
    public decimal? NumericAmount { get; set; }
    public string Campaign { get; set; } = string.Empty;
    public string Received { get; set; } = string.Empty; // e.g. "22 Jun"
    public string ReceivedDate { get; set; } = string.Empty; // ISO date string e.g. "2025-06-22"
    public string Verification { get; set; } = "Pending"; // Verified | Pending | Flagged
    public string Allocation { get; set; } = "Unallocated"; // Allocated | Partially Allocated | Unallocated | On Hold
    public string? Notes { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? UpdatedAt { get; set; }
}
