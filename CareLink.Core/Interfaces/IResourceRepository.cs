using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IResourceRepository
{
    Task<ResourceItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ResourceItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<ResourceItem> Items, int TotalCount)> GetPagedAsync(ResourceQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<(int HealthyCount, int LowStockCount, int CriticalCount, int OutOfStockCount, int ImmediateActionCount, int ExpiringSoonCount, int TotalItemsCount)> GetSummaryMetricsAsync(CancellationToken cancellationToken = default);
    Task<ResourceItem> AddAsync(ResourceItem item, CancellationToken cancellationToken = default);
    Task<ResourceItem?> UpdateAsync(ResourceItem item, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<ResourceDistribution> AddDistributionAsync(ResourceDistribution distribution, CancellationToken cancellationToken = default);
    Task<IEnumerable<ResourceDistribution>> GetDistributionsAsync(string? resourceId = null, CancellationToken cancellationToken = default);
}
