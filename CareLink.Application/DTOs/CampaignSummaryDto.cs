namespace CareLink.Application.DTOs;

public class CampaignSummaryDto
{
    public int ActiveCampaigns { get; set; }
    public decimal CombinedTarget { get; set; }
    public decimal TotalRaised { get; set; }
    public int EndingSoon { get; set; }
    public decimal FundingProgressPercentage => CombinedTarget > 0 ? Math.Round((TotalRaised / CombinedTarget) * 100, 1) : 0;
}
