namespace CareLink.Core.Models;

public class TransparencyReportQueryParameters
{
    public string? Search { get; set; }
    public string? Type { get; set; } // Weekly, Campaign, Material, Partner, All
    public string? Status { get; set; } // Published, Draft, Under Review
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
