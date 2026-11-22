namespace CareLink.Application.DTOs;

public class FundAllocationDto
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public DateTime AllocationDate { get; set; }
    public string Approval { get; set; } = string.Empty;
    public string Disbursement { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
