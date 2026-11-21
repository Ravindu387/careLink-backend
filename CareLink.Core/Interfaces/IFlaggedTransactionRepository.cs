using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public class FlaggedTransactionSummaryMetrics
{
    public int TotalCount { get; set; }
    public int OpenFlags { get; set; }        // Investigation Required + Under Review + Deferred
    public int HighPriority { get; set; }     // High severity + not Resolved
    public int UnderReview { get; set; }
    public int Resolved { get; set; }
    public decimal HighPriorityCombinedAmount { get; set; }
}

public interface IFlaggedTransactionRepository
{
    Task<(IEnumerable<FlaggedTransaction> Items, int TotalCount)> GetPagedAsync(FlaggedTransactionQueryParameters parameters);
    Task<IEnumerable<FlaggedTransaction>> GetAllAsync();
    Task<FlaggedTransaction?> GetByIdAsync(string id);
    Task<FlaggedTransaction> AddAsync(FlaggedTransaction flag);
    Task<FlaggedTransaction> UpdateAsync(FlaggedTransaction flag);
    Task<bool> DeleteAsync(string id);
    Task<FlaggedTransactionSummaryMetrics> GetSummaryMetricsAsync();
    Task<string> GenerateNextIdAsync();
}
