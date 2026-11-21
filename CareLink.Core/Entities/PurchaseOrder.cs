namespace CareLink.Core.Entities;

public class PurchaseOrder
{
    public string Id { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public string Items { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "Standard";
    public string DeliveryDue { get; set; } = string.Empty;
    public string Status { get; set; } = "Awaiting Approval";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
