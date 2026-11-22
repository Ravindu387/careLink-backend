using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IResourceContributionRepository
{
    Task<(IEnumerable<ResourceContribution> Items, int TotalCount)> GetPagedAsync(
        ResourceContributionQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceContribution>> GetAllAsync(
        ResourceContributionQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ResourceContribution?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<ResourceContribution> AddAsync(ResourceContribution contribution, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(ResourceContribution contribution, CancellationToken cancellationToken = default);

    Task<bool> UpdateVerificationAsync(string id, string verification, CancellationToken cancellationToken = default);

    Task<bool> UpdateAllocationAsync(string id, string allocation, CancellationToken cancellationToken = default);

    Task<int> BulkVerifyAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
