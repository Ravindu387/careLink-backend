using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IResourceSharingService
{
    Task<IReadOnlyList<ResourceSharingCommitmentDto>> GetAllAsync(ResourceSharingQueryParameters query, CancellationToken cancellationToken = default);
    Task<ResourceSharingCommitmentDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ResourceSharingCommitmentDto> CreateAsync(CreateResourceSharingCommitmentDto dto, CancellationToken cancellationToken = default);
    Task<ResourceSharingCommitmentDto?> UpdateAsync(string id, UpdateResourceSharingCommitmentDto dto, CancellationToken cancellationToken = default);
    Task<ResourceSharingCommitmentDto?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<ResourceSharingMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);
    Task ResetAsync(CancellationToken cancellationToken = default);
}
