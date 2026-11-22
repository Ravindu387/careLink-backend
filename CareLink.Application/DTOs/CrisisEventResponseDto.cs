namespace CareLink.Application.DTOs;

public class CrisisEventResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Sectors { get; set; } = "Food · WASH · Shelter";
    public string Event => Name;
    public string Date => StartDate;
    public string Requests { get; set; } = "0";
    public string Coordinator { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long AffectedPeopleCount { get; set; }
    public int DistrictsCovered { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
