using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public class PriorityQueueFilterParams
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? CrisisEvent { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public string? Organisation { get; set; }
    public bool? OverdueOnly { get; set; }
    public bool? UnassignedOnly { get; set; }
}

public interface IPriorityQueueRepository
{
    Task<IReadOnlyList<PriorityQueueItem>> GetAllAsync(
        PriorityQueueFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<PriorityQueueItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<PriorityQueueItem> AddAsync(PriorityQueueItem item, CancellationToken cancellationToken = default);

    Task<PriorityQueueItem?> UpdateAsync(PriorityQueueItem item, CancellationToken cancellationToken = default);

    Task<PriorityQueueItem?> AssignTeamAsync(string id, string assignedTeam, CancellationToken cancellationToken = default);

    Task<PriorityQueueItem?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default);

    Task<PriorityQueueItem?> UpdatePriorityAsync(
        string id,
        string priorityLevel,
        int confidence,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<PriorityQueueMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SlaPolicy>> GetSlaPoliciesAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
