using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IResourceService
{
    // Operational & Coordinator Methods (Safna)
    Task<IReadOnlyList<ResourceDto>> GetAllAsync(
        ResourceQueryParameters? query = null,
        CancellationToken cancellationToken = default);

    Task<ResourceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ResourceDto> CreateItemAsync(CreateResourceItemDto dto, CancellationToken cancellationToken = default);

    Task<ResourceDto?> UpdateStockAsync(int id, UpdateResourceStockDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<ResourceMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task<ResourceRequestResponseDto> CreateRequestAsync(CreateResourceRequestDto dto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResourceRequestResponseDto>> GetRequestsAsync(CancellationToken cancellationToken = default);

    Task ResetDefaultDataAsync(CancellationToken cancellationToken = default);

    // Shelter Inventory Management Methods (Prabhani)
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
