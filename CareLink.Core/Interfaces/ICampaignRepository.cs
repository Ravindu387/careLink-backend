using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface ICampaignRepository
{
    Task<IEnumerable<Campaign>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Campaign> Items, int TotalCount)> GetPagedAsync(CampaignQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<Campaign?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Campaign> AddAsync(Campaign campaign, CancellationToken cancellationToken = default);
    Task<Campaign?> UpdateAsync(Campaign campaign, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<(int ActiveCampaigns, decimal CombinedTarget, decimal TotalRaised, int EndingSoon)> GetSummaryMetricsAsync(CancellationToken cancellationToken = default);
}
