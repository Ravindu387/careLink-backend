namespace CareLink.Core.Models;

public class AccommodationRequestQueryParameters
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? ShelterId { get; set; }
    public string? DateRange { get; set; }
    public int? MinFamilyMembers { get; set; }
    public int? MaxFamilyMembers { get; set; }
    public string? Location { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 8;
}
