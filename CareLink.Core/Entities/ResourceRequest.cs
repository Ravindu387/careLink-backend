namespace CareLink.Core.Entities;

public class ResourceRequest
{
    public string Id { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;

    private string _resource = string.Empty;
    public string Resource
    {
        get => !string.IsNullOrEmpty(ResourceName) ? ResourceName : _resource;
        set
        {
            _resource = value;
            ResourceName = value;
        }
    }

    public string Quantity { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Source { get; set; } = "Central Hub";
    public string OperationalArea
    {
        get => Source;
        set => Source = value;
    }
    public string Age { get; set; } = "Just now";
    public string Priority { get; set; } = "Critical"; // Normal, High, Critical
    public string Status { get; set; } = "Awaiting Approval"; // Pending, Approved, Dispatched, Fulfilled, Rejected, Awaiting Approval, Allocated, Partially Available
    public string RequestedBy { get; set; } = "Emergency Coordinator";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FulfilledAt { get; set; }
}
