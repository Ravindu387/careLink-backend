namespace CareLink.Application.DTOs;

public class FlaggedTransactionSummaryDto
{
    public int TotalCount { get; set; }
    public int OpenFlags { get; set; }
    public int HighPriority { get; set; }
    public int UnderReview { get; set; }
    public int Resolved { get; set; }
    public decimal HighPriorityCombinedAmount { get; set; }
}
