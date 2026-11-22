namespace CareLink.Application.DTOs;

public class DonorSummaryDto
{
    public int TotalDonors { get; set; }
    public int ActiveDonors { get; set; }
    public int VerifiedDonors { get; set; }
    public int UnderReviewDonors { get; set; }
    public int RestrictedDonors { get; set; }
    public decimal TotalContributions { get; set; }
}
