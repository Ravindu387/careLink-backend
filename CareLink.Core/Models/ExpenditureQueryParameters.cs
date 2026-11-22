namespace CareLink.Core.Models;

public class ExpenditureQueryParameters
{
    public string? Search { get; set; }
    public string? Type { get; set; }
    public string? Verification { get; set; }
    public string? PaymentStatus { get; set; }
    public string? DateRange { get; set; }   // "Last 7 days" | "Last 30 days" | "Last 90 days"
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
