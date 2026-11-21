namespace CareLink.Application.DTOs;

public class OccupancySummaryDto
{
    public int TotalCapacity { get; set; }
    public int TotalOccupied { get; set; }
    public int TotalReserved { get; set; }
    public int TotalAvailable { get; set; }
    public double OccupancyRate { get; set; }
    public int TotalAdmissionsToday { get; set; }
    public int TotalDeparturesToday { get; set; }
    public int WarningSheltersCount { get; set; }
    public int CriticalSheltersCount { get; set; }
    public int ShelterCount { get; set; }
    public double WarningThreshold { get; set; }
    public double CriticalThreshold { get; set; }
}
