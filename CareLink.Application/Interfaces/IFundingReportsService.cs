using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IFundingReportsService
{
    Task<FundingAnalyticsReportDto> GetAnalyticsAsync(FundingAnalyticsQueryParameters parameters, CancellationToken cancellationToken = default);
}
