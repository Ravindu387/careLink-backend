namespace CareLink.Application.DTOs;

public class ExpenditureDto
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Invoice { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public string Verification { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
