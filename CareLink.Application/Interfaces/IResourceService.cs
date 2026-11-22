using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IResourceService
{
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
}
