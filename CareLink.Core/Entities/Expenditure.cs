namespace CareLink.Core.Entities;

public class Expenditure
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Invoice { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;          // Display: "21 Jun"
    public DateTime ExpenseDate { get; set; }
    public string Verification { get; set; } = "Pending Review"; // Verified | Pending Review | Rejected
    public string PaymentStatus { get; set; } = "On Hold";       // Paid | On Hold | Pending
    public string Type { get; set; } = string.Empty;             // Shelter | Transport | Medical | Water | Other
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
