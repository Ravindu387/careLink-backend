namespace CareLink.Core.Models;

public class DonorQueryParameters
{
    public string? Search { get; set; }
    public string? Type { get; set; } // Foundation | Corporate | International NGO | Community Group | Individual
    public string? Verification { get; set; } // Verified | Under Review | Pending
    public string? Status { get; set; } // Active | Restricted | Inactive
    public string? Scope { get; set; } // International | National | Greater Riverton
    public string? DateRange { get; set; } // Last 7 days | Last 30 days | Last 90 days | This year
    public decimal? MinContribution { get; set; }
    public decimal? MaxContribution { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
