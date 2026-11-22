using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public class AuditFilterParams
{
    public string? Search { get; set; }
    public string? Action { get; set; }
    public string? ActionType { get; set; }
    public string? Actor { get; set; }
    public string? Type { get; set; }
    public string? RecordType { get; set; }
    public string? DateRange { get; set; }
    public string? DateFrom { get; set; }
    public string? DateTo { get; set; }
}

public interface IAuditTimelineRepository
{
    Task<IReadOnlyList<AuditEvent>> GetAllAsync(
        AuditFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<AuditEvent?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<AuditEvent> AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);

    Task<AuditTimelineMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task<bool> VerifyLogIntegrityAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
