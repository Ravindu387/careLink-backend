using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public class ExpenditureSummaryMetrics
{
    public decimal TotalExpenditure { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public decimal TotalOnHold { get; set; }
    public int PendingReviewCount { get; set; }
    public int VerifiedCount { get; set; }
    public int RejectedCount { get; set; }
    public int PaidCount { get; set; }
    public int OnHoldCount { get; set; }
    public int TotalCount { get; set; }
    public Dictionary<string, decimal> ByType { get; set; } = new();
}

public interface IExpenditureRepository
{
    Task<(IEnumerable<Expenditure> Items, int TotalCount)> GetPagedAsync(ExpenditureQueryParameters parameters);
    Task<IEnumerable<Expenditure>> GetAllAsync();
    Task<Expenditure?> GetByIdAsync(string id);
    Task<Expenditure> AddAsync(Expenditure expenditure);
    Task<Expenditure> UpdateAsync(Expenditure expenditure);
    Task<bool> DeleteAsync(string id);
    Task<int> BulkUpdateAsync(IEnumerable<string> ids, string action);
    Task<ExpenditureSummaryMetrics> GetSummaryMetricsAsync();
    Task<string> GenerateNextIdAsync();
}
