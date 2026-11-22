using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IResourceContributionService
{
    Task<PagedResultDto<ResourceContributionDto>> GetPagedAsync(
        ResourceContributionQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceContributionDto>> GetAllAsync(
        ResourceContributionQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ResourceContributionDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<ResourceContributionDto> CreateAsync(CreateResourceContributionDto dto, CancellationToken cancellationToken = default);

    Task<ResourceContributionDto?> UpdateAsync(string id, UpdateResourceContributionDto dto, CancellationToken cancellationToken = default);

    Task<bool> UpdateVerificationAsync(string id, string verification, CancellationToken cancellationToken = default);

    Task<bool> UpdateAllocationAsync(string id, string allocation, CancellationToken cancellationToken = default);

    Task<int> BulkVerifyAsync(List<string> ids, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
