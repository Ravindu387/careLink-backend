using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IResourceService
{
    Task<PagedResultDto<ResourceDto>> GetPagedResourcesAsync(ResourceQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<IEnumerable<ResourceDto>> GetAllResourcesAsync(CancellationToken cancellationToken = default);
    Task<ResourceDto?> GetResourceByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ResourceSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<ResourceDto> CreateResourceAsync(CreateResourceDto dto, CancellationToken cancellationToken = default);
    Task<ResourceDto?> UpdateResourceAsync(string id, UpdateResourceDto dto, CancellationToken cancellationToken = default);
    Task<ResourceDto?> RecordDistributionAsync(RecordDistributionDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<ResourceDistributionDto>> GetDistributionsAsync(string? resourceId = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteResourceAsync(string id, CancellationToken cancellationToken = default);
}
