namespace CareLink.Core.Models;

public class ResourceContributionQueryParameters
{
    public string? Search { get; set; }
    public string? Type { get; set; }
    public string? Verification { get; set; }
    public string? Status { get; set; } // Matches Allocation
    public string? Campaign { get; set; }
    public string? DateRange { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
