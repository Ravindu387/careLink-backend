namespace CareLink.Core.Entities;

public class FundAllocation
{
    public string Id { get; set; } = string.Empty; // e.g. "ALL-4408"
    public decimal Amount { get; set; }
    public string Source { get; set; } = string.Empty; // "Flood Response 2025", "Medical Relief", "Clean Water Appeal", "Emergency Shelter Fund"
    public string Purpose { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty; // Display date e.g. "22 Jun"
    public DateTime AllocationDate { get; set; }
    public string Approval { get; set; } = "Under Review"; // "Approved" | "Under Review" | "Rejected"
    public string Disbursement { get; set; } = "Pending"; // "Disbursed" | "68% Disbursed" | "Pending" | "Not Disbursed"
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
