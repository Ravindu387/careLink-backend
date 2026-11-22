using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public interface IResourceSharingRepository
{
    Task<IReadOnlyList<ResourceSharingCommitment>> GetAllAsync(ResourceSharingQuery query, CancellationToken cancellationToken = default);
    Task<ResourceSharingCommitment?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ResourceSharingCommitment> CreateAsync(ResourceSharingCommitment commitment, CancellationToken cancellationToken = default);
    Task<ResourceSharingCommitment?> UpdateAsync(ResourceSharingCommitment commitment, CancellationToken cancellationToken = default);
    Task<ResourceSharingCommitment?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<ResourceSharingMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);
    Task ResetAsync(CancellationToken cancellationToken = default);
}
