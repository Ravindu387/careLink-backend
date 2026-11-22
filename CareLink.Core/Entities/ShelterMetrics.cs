namespace CareLink.Core.Entities;

public class ShelterMetrics
{
    public int OperationalShelters { get; set; }
    public int TotalCapacity { get; set; }
    public int TotalOccupied { get; set; }
    public int AvailableSpaces { get; set; }
    public int AcceptingArrivals { get; set; }
    public int NearCapacityCount { get; set; }
    public int FullCount { get; set; }
    public int TotalShelters { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
