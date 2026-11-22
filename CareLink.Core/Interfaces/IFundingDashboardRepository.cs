using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IFundingDashboardRepository
{
    Task<FundingDashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default);
}
