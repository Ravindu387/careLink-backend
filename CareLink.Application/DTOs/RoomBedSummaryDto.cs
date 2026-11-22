namespace CareLink.Application.DTOs;

public class RoomBedSummaryDto
{
    public int TotalRooms { get; set; }
    public int TotalBeds { get; set; }
    public int AvailableBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public int ReservedBeds { get; set; }
    public int UnavailableBeds { get; set; }
}
