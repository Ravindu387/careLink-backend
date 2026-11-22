using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IFundingDashboardService
{
    Task<FundingDashboardDto> GetDashboardDataAsync(CancellationToken cancellationToken = default);
}
