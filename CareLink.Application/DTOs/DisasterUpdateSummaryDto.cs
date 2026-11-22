namespace CareLink.Application.DTOs;

public class DisasterUpdateSummaryDto
{
    public int TotalActive { get; set; }
    public int CriticalCount { get; set; }
    public int WarningCount { get; set; }
    public int AnnouncementCount { get; set; }
    public int UpdateCount { get; set; }
    public int ArchivedCount { get; set; }
    public int DraftCount { get; set; }
    public DisasterUpdateDto? LatestCriticalAlert { get; set; }
}
