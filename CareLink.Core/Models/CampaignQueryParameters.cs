namespace CareLink.Core.Models;

public class CampaignQueryParameters
{
    public string? Search { get; set; }
    public string? Event { get; set; }
    public string? Status { get; set; }
    public string? Progress { get; set; } // Below 50% | 50% - 75% | Above 75%
    public decimal? MinTarget { get; set; }
    public decimal? MaxTarget { get; set; }
    public bool? IncludeArchived { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
