namespace CareLink.Application.DTOs;

public class DonationSummaryDto
{
    public int TotalRecords { get; set; }
    public int PendingVerificationCount { get; set; }
    public int MonetaryCount { get; set; }
    public int MaterialCount { get; set; }
    public int FlaggedCount { get; set; }
    public decimal TotalMonetaryValue { get; set; }
    public int AllocatedCount { get; set; }
}
