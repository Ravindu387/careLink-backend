using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class FundingDashboardService : IFundingDashboardService
{
    private readonly IFundingDashboardRepository _repository;

    public FundingDashboardService(IFundingDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<FundingDashboardDto> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        var data = await _repository.GetDashboardDataAsync(cancellationToken);

        return new FundingDashboardDto
        {
            FinancialStats = data.FinancialStats,
            WeeklyChart = data.WeeklyChart,
            WeeklyReceivedTotal = data.WeeklyReceivedTotal,
            WeeklyReceivedDisplay = data.WeeklyReceivedDisplay,
            CampaignPerformance = data.CampaignPerformance,
            RecentDonations = data.RecentDonations,
            AttentionItems = data.AttentionItems
        };
    }
}
