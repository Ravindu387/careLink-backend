using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public class SituationReportFilterParams
{
    public string? Search { get; set; }
    public string? Event { get; set; }
    public string? Status { get; set; }
    public string? DateRange { get; set; }
}

public interface ISituationReportRepository
{
    Task<IReadOnlyList<SituationReport>> GetAllAsync(
        SituationReportFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<SituationReport?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<SituationReport> AddAsync(SituationReport report, CancellationToken cancellationToken = default);

    Task<SituationReport?> UpdateStatusAsync(
        string id,
        string status,
        string approvedBy,
        int recipients,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<SituationReportMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
