namespace CareLink.Application.DTOs;

public class ExpenditureSummaryDto
{
    public decimal TotalExpenditure { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public decimal TotalOnHold { get; set; }
    public int PendingReviewCount { get; set; }
    public int VerifiedCount { get; set; }
    public int RejectedCount { get; set; }
    public int PaidCount { get; set; }
    public int OnHoldCount { get; set; }
    public int TotalCount { get; set; }
    public Dictionary<string, decimal> ByType { get; set; } = new();
}
