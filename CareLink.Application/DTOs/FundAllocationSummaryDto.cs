namespace CareLink.Application.DTOs;

public class FundAllocationSummaryDto
{
    public decimal TotalAllocated { get; set; }
    public decimal TotalDisbursed { get; set; }
    public decimal TotalPending { get; set; }
    public int ApprovedCount { get; set; }
    public int UnderReviewCount { get; set; }
    public int RejectedCount { get; set; }
    public int DisbursedCount { get; set; }
    public int TotalCount { get; set; }
    public Dictionary<string, decimal> BySource { get; set; } = new();
}
