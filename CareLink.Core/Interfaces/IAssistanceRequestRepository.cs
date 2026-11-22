using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public class AssistanceRequestFilterParams
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? CrisisEvent { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public string? Organisation { get; set; }
    public bool? VerifiedOnly { get; set; }
    public bool? UnassignedOnly { get; set; }
}

public interface IAssistanceRequestRepository
{
    Task<IReadOnlyList<AssistanceRequest>> GetAllAsync(
        AssistanceRequestFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<AssistanceRequest?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<AssistanceRequest> AddAsync(AssistanceRequest request, CancellationToken cancellationToken = default);

    Task<AssistanceRequest?> UpdateAsync(AssistanceRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<AssistanceRequestMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
