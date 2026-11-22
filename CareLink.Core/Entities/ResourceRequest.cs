namespace CareLink.Core.Entities;

public class ResourceRequest
{
    public string Id { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string OperationalArea { get; set; } = "Central Depot";
    public string Priority { get; set; } = "Normal"; // Normal, High, Critical
    public string Status { get; set; } = "Pending"; // Pending, Approved, Dispatched, Fulfilled, Rejected
    public string RequestedBy { get; set; } = "Emergency Coordinator";
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FulfilledAt { get; set; }
}
