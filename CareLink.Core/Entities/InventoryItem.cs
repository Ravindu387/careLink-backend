namespace CareLink.Core.Entities;

public class InventoryItem
{
    public string Id { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public int OnHand { get; set; }
    public int Reserved { get; set; }
    public int InTransit { get; set; }
    public int Available { get; set; }
    public string Status { get; set; } = "Healthy";
    public string Unit { get; set; } = "units";
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
