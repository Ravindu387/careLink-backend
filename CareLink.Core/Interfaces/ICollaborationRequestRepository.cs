using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public class CollaborationFilterParams
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? Sector { get; set; }
    public string? Partner { get; set; }
    public string? Area { get; set; }
    public string? Status { get; set; }
}

public interface ICollaborationRequestRepository
{
    Task<IReadOnlyList<CollaborationRequest>> GetAllAsync(
        CollaborationFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<CollaborationRequest?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<CollaborationRequest> AddAsync(CollaborationRequest collaboration, CancellationToken cancellationToken = default);

    Task<CollaborationRequest?> UpdateAsync(CollaborationRequest collaboration, CancellationToken cancellationToken = default);

    Task<CollaborationRequest?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<CollaborationRequestMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
