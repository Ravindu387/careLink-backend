using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IDonorRepository
{
    Task<IEnumerable<Donor>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Donor> Items, int TotalCount)> GetPagedAsync(DonorQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<Donor?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Donor> AddAsync(Donor donor, CancellationToken cancellationToken = default);
    Task<Donor?> UpdateAsync(Donor donor, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<int> BulkUpdateStatusAsync(IEnumerable<string> ids, string status, CancellationToken cancellationToken = default);
    Task<DonorSummaryMetrics> GetSummaryMetricsAsync(CancellationToken cancellationToken = default);
}

public class DonorSummaryMetrics
{
    public int TotalDonors { get; set; }
    public int ActiveDonors { get; set; }
    public int VerifiedDonors { get; set; }
    public int UnderReviewDonors { get; set; }
    public int RestrictedDonors { get; set; }
    public decimal TotalContributions { get; set; }
}
