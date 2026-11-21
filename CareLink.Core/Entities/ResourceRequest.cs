namespace CareLink.Core.Entities;

public class ResourceRequest
{
    public string Id { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Priority { get; set; } = "Critical";
    public string Category { get; set; } = string.Empty;
    public string Source { get; set; } = "Central Hub";
    public string Age { get; set; } = "Just now";
    public string Status { get; set; } = "Awaiting Approval";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
