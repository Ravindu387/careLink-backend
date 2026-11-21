using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public class ResourceFilterParams
{
    public string? Search { get; set; }
    public string? Area { get; set; }
    public string? Status { get; set; }
    public string? Availability { get; set; } // Above 60%, Below 60%, Any
    public string? Category { get; set; }
}

public interface IResourceRepository
{
    Task<IReadOnlyList<ResourceItem>> GetAllAsync(
        ResourceFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<ResourceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ResourceItem> AddAsync(ResourceItem item, CancellationToken cancellationToken = default);

    Task<ResourceItem?> UpdateAsync(ResourceItem item, CancellationToken cancellationToken = default);

    Task<ResourceItem?> UpdateStockAsync(
        int id,
        int? available,
        int? reserved,
        string? status,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<ResourceMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task<ResourceRequest> CreateRequestAsync(ResourceRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResourceRequest>> GetRequestsAsync(CancellationToken cancellationToken = default);

    Task ResetDefaultDataAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default) => ResetDefaultDataAsync(cancellationToken);
}
