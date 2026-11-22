namespace CareLink.Core.Entities;

public class ShelterMetrics
{
    public int OperationalShelters { get; set; }
    public int OperationalCount { get => OperationalShelters; set => OperationalShelters = value; }
    public int TotalCapacity { get; set; }
    public int TotalOccupied { get; set; }
    public int CurrentOccupancy { get => TotalOccupied; set => TotalOccupied = value; }
    public int AvailableSpaces { get; set; }
    public int AvailableCapacity { get => AvailableSpaces; set => AvailableSpaces = value; }
    public int AcceptingArrivals { get; set; }
    public int AcceptingCount { get => AcceptingArrivals; set => AcceptingArrivals = value; }
    public int NearCapacityCount { get; set; }
    public int FullCount { get; set; }
    public int TotalShelters { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
