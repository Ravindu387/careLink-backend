namespace CareLink.Application.DTOs;

public class ReportsMetricsDto
{
    public double Occupancy { get; set; }
    public int Admissions { get; set; }
    public double Approved { get; set; }
    public int ApprovalCount { get; set; }
    public int Resources { get; set; }
    public int Pending { get; set; }
    public int Rejected { get; set; }
    public int Departed { get; set; }
    public string OccupancyChange { get; set; } = "+4.8% in period";
}
