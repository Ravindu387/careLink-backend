namespace CareLink.Application.DTOs;

public class CrisisEventMetricsDto
{
    public int ActiveEvents { get; set; }
    public int MajorSeverityEvents { get; set; }
    public long TotalAffectedPeople { get; set; }
    public string FormattedAffectedPeople { get; set; } = "184K";
    public int AssignedCoordinators { get; set; }
    public int TotalEvents { get; set; }
    public int MonitoringEvents { get; set; }
    public int ResolvedEvents { get; set; }
    public int DistrictsCount { get; set; } = 6;
}
