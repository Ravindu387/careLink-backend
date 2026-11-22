using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IDonationRepository
{
    Task<IEnumerable<DonationRecord>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<DonationRecord> Items, int TotalCount)> GetPagedAsync(DonationQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<DonationRecord?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<DonationRecord> AddAsync(DonationRecord donation, CancellationToken cancellationToken = default);
    Task<DonationRecord?> UpdateAsync(DonationRecord donation, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<int> BulkUpdateAsync(IEnumerable<string> ids, string? verification, string? allocation, CancellationToken cancellationToken = default);
    Task<DonationSummaryMetrics> GetSummaryMetricsAsync(CancellationToken cancellationToken = default);
}

public class DonationSummaryMetrics
{
    public int TotalRecords { get; set; }
    public int PendingVerificationCount { get; set; }
    public int MonetaryCount { get; set; }
    public int MaterialCount { get; set; }
    public int FlaggedCount { get; set; }
    public decimal TotalMonetaryValue { get; set; }
    public int AllocatedCount { get; set; }
}
