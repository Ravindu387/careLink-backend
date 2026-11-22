namespace CareLink.Core.Entities;

public class NearbyFacility
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Distance { get; set; } = string.Empty;
    public double DistanceKm { get; set; } = 1.0;
    public string Hours { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "green";
    public double MapX { get; set; } = 50;
    public double MapY { get; set; } = 50;
    public string IconKey { get; set; } = "House";
    public List<string> Services { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
