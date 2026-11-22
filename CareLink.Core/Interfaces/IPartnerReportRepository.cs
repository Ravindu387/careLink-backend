using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public class PartnerReportFilterParams
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
}

public interface IPartnerReportRepository
{
    Task<IReadOnlyList<PartnerReport>> GetAllAsync(
        PartnerReportFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<PartnerReport?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<PartnerReport> AddAsync(
        PartnerReport report,
        CancellationToken cancellationToken = default);

    Task<PartnerReport?> UpdateAsync(
        PartnerReport report,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(
        string id,
        string status,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<PartnerReportsAnalytics> GetAnalyticsAsync(
        CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(
        CancellationToken cancellationToken = default);
}
