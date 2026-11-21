namespace CareLink.Core.Models;

public class DonationQueryParameters
{
    public string? Search { get; set; }
    public string? Tab { get; set; } // "All records" | "Pending verification" | "Monetary" | "Material" | "Flagged"
    public string? Type { get; set; } // "Monetary" | "Material"
    public string? Verification { get; set; } // "Verified" | "Pending" | "Flagged"
    public string? Allocation { get; set; } // "Allocated" | "Partially Allocated" | "Unallocated" | "On Hold"
    public string? Campaign { get; set; }
    public string? DateRange { get; set; } // "Last 7 days" | "Last 30 days" | "Last 90 days" | "This year"
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
