namespace CareLink.Core.Models;

public class FundAllocationQueryParameters
{
    public string? Search { get; set; }
    public string? Source { get; set; }
    public string? Approval { get; set; }
    public string? Disbursement { get; set; }
    public string? DateRange { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
