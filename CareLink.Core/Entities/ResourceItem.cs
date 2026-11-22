namespace CareLink.Core.Entities;

public class ResourceItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Food & Water"; // Food & Water, Accommodation, Medical, Logistics
    public string Location { get; set; } = "Central Depot"; // Central Depot, North Hub, Eastbank Store
    public int Available { get; set; }
    public int Reserved { get; set; }
    public int Availability { get; set; }
    public int Minimum { get; set; } = 100;
    public int MinimumThreshold
    {
        get => Minimum;
        set => Minimum = value;
    }
    public string Status { get; set; } = "Healthy"; // Healthy, Low Stock, Critical, Out of Stock
    public string Unit { get; set; } = "Units";
    public DateTime? ExpiryDate { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public DateTimeOffset LastRestocked { get; set; } = DateTimeOffset.UtcNow;
    public string ShelterId { get; set; } = string.Empty;
    public string ShelterName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
