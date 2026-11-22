namespace CareLink.Core.Entities;

public class Allocation
{
    public string Id { get; set; } = string.Empty;
    public string Request { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public string ApprovedAt { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string Warehouse { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
