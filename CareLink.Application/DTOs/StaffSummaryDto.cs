namespace CareLink.Application.DTOs;

public class StaffSummaryDto
{
    public int TotalStaff { get; set; }
    public int OnDuty { get; set; }
    public int NextShift { get; set; }
    public int Unavailable { get; set; }
}
