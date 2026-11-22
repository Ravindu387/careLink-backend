using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public class FieldActivityFilterParams
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? Sector { get; set; }
    public string? Location { get; set; }
    public string? Team { get; set; }
    public string? Status { get; set; }
}

public interface IFieldActivityRepository
{
    Task<IReadOnlyList<FieldActivity>> GetAllAsync(
        FieldActivityFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<FieldActivity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<FieldActivity> AddAsync(FieldActivity activity, CancellationToken cancellationToken = default);

    Task<FieldActivity?> UpdateAsync(FieldActivity activity, CancellationToken cancellationToken = default);

    Task<FieldActivity?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<FieldActivityMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
