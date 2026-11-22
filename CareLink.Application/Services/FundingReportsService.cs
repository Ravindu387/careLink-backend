using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class FundingReportsService : IFundingReportsService
{
    private readonly IFundingReportsRepository _repository;

    public FundingReportsService(IFundingReportsRepository repository)
    {
        _repository = repository;
    }

    public async Task<FundingAnalyticsReportDto> GetAnalyticsAsync(
        FundingAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var data = await _repository.GetAnalyticsAsync(parameters, cancellationToken);

        return new FundingAnalyticsReportDto
        {
            Metrics = data.Metrics,
            Trend = data.Trend,
            Sources = data.Sources,
            CombinedSourceValue = data.CombinedSourceValue,
            CombinedSourceDisplay = data.CombinedSourceDisplay,
            AvailableCampaigns = data.AvailableCampaigns
        };
    }
}
