using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IFundingReportsRepository
{
    Task<FundingAnalyticsReport> GetAnalyticsAsync(FundingAnalyticsQueryParameters parameters, CancellationToken cancellationToken = default);
}
