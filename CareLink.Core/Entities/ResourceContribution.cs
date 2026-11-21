namespace CareLink.Core.Entities;

public class ResourceContribution
{
    public string Id { get; set; } = string.Empty;
    public string Donor { get; set; } = string.Empty;
    public string Type { get; set; } = "Material"; // Material, Monetary
    public string Quantity { get; set; } = string.Empty;
    public string Campaign { get; set; } = string.Empty;
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    public string Received { get; set; } = string.Empty; // e.g. "21 Jun"
    public string Verification { get; set; } = "Pending"; // Verified, Pending, Flagged
    public string Allocation { get; set; } = "Unallocated"; // Allocated, Partially Allocated, Unallocated
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
