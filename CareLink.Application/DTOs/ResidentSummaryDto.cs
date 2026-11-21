namespace CareLink.Application.DTOs;

public class ResidentSummaryDto
{
    public int TotalRegistered { get; set; }
    public int TotalShelters { get; set; }
    public int FamilyGroupsCount { get; set; }
    public int SingleResidentsCount { get; set; }
    public int AssistanceNeedsCount { get; set; }
    public int CheckedOutTodayCount { get; set; }
    public int CheckedInTodayCount { get; set; }
}
