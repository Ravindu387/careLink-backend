using CareLink.Core.Models;

namespace CareLink.Application.DTOs;

public class FundingDashboardDto
{
    public FundingFinancialStats FinancialStats { get; set; } = new();
    public List<FundingWeeklyChartItem> WeeklyChart { get; set; } = new();
    public decimal WeeklyReceivedTotal { get; set; }
    public string WeeklyReceivedDisplay { get; set; } = string.Empty;
    public List<FundingCampaignPerformanceItem> CampaignPerformance { get; set; } = new();
    public List<FundingRecentDonationItem> RecentDonations { get; set; } = new();
    public List<FundingAttentionItem> AttentionItems { get; set; } = new();
}
