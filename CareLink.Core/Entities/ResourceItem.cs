namespace CareLink.Core.Entities;

public class ResourceItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Food & Water"; // Food & Water, Accommodation, Medical, Logistics
    public string Location { get; set; } = "Central Depot"; // Central Depot, North Hub, Eastbank Store
    public int Available { get; set; }
    public int Reserved { get; set; }
    public int Availability { get; set; }
    public string Status { get; set; } = "Healthy"; // Healthy, Low Stock, Critical
    public int MinimumThreshold { get; set; } = 100;
    public string Unit { get; set; } = "Units";
    public DateTimeOffset LastRestocked { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
