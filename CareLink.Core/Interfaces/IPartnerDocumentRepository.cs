using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public class DocumentFilterParams
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? Type { get; set; }
    public string? Owner { get; set; }
    public string? Visibility { get; set; }
    public string? Status { get; set; }
}

public interface IPartnerDocumentRepository
{
    Task<IReadOnlyList<PartnerDocument>> GetAllAsync(
        DocumentFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<PartnerDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<PartnerDocument> AddAsync(PartnerDocument document, CancellationToken cancellationToken = default);

    Task<PartnerDocument?> UpdateAsync(PartnerDocument document, CancellationToken cancellationToken = default);

    Task<PartnerDocument?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<PartnerDocumentMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
