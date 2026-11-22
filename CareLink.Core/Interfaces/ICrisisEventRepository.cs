using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public class CrisisEventFilterParams
{
    public string? Search { get; set; }
    public string? Type { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
    public string? DateRange { get; set; }
    public bool? ShowArchived { get; set; }
}

public interface ICrisisEventRepository
{
    Task<IReadOnlyList<CrisisEvent>> GetAllAsync(
        CrisisEventFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<CrisisEvent?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<CrisisEvent> AddAsync(CrisisEvent crisisEvent, CancellationToken cancellationToken = default);

    Task<CrisisEvent?> UpdateAsync(CrisisEvent crisisEvent, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<CrisisEventMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);

    Task<CrisisAccessRequest> AddAccessRequestAsync(CrisisAccessRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CrisisAccessRequest>> GetAccessRequestsAsync(CancellationToken cancellationToken = default);
}
