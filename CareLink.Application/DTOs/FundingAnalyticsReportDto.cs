using CareLink.Core.Models;

namespace CareLink.Application.DTOs;

public class FundingAnalyticsReportDto
{
    public FundingAnalyticsMetrics Metrics { get; set; } = new();
    public FundingAnalyticsTrend Trend { get; set; } = new();
    public List<FundingContributionSource> Sources { get; set; } = new();
    public decimal CombinedSourceValue { get; set; }
    public string CombinedSourceDisplay { get; set; } = string.Empty;
    public List<string> AvailableCampaigns { get; set; } = new();
}
