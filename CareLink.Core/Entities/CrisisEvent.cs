namespace CareLink.Core.Entities;

public class CrisisEvent
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Flood";
    public string Severity { get; set; } = "Major · Level 4";
    public int SeverityLevel { get; set; } = 4;
    public string Area { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.UtcNow;
    public string Status { get; set; } = "Active";
    public int RequestCount { get; set; }
    public string Coordinator { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long AffectedPeopleCount { get; set; } = 1000;
    public int DistrictsCovered { get; set; } = 1;
    public string Sectors { get; set; } = "Food · WASH · Shelter";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
