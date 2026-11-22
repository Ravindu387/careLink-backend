using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public class OperationPointFilterParams
{
    public string? Search { get; set; }
    public string? District { get; set; }
    public string? Layer { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
}

public interface ILiveOperationsMapRepository
{
    Task<IReadOnlyList<OperationPoint>> GetAllAsync(
        OperationPointFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<OperationPoint?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<OperationPoint> AddAsync(OperationPoint point, CancellationToken cancellationToken = default);

    Task<OperationPoint?> UpdateAsync(OperationPoint point, CancellationToken cancellationToken = default);

    Task<OperationPoint?> AssignTeamAsync(
        string id,
        string assignedTeam,
        string? status = null,
        string? notes = null,
        CancellationToken cancellationToken = default);

    Task<OperationPoint?> UpdateCoordinatesAsync(
        string id,
        double x,
        double y,
        double? latitude = null,
        double? longitude = null,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<LiveOperationsMapMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetDefaultDataAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default) => ResetDefaultDataAsync(cancellationToken);
}
