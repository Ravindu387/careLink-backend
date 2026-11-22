using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public class FundAllocationSummaryMetrics
{
    public decimal TotalAllocated { get; set; }
    public decimal TotalDisbursed { get; set; }
    public decimal TotalPending { get; set; }
    public int ApprovedCount { get; set; }
    public int UnderReviewCount { get; set; }
    public int RejectedCount { get; set; }
    public int DisbursedCount { get; set; }
    public int TotalCount { get; set; }
    public Dictionary<string, decimal> BySource { get; set; } = new();
}

public interface IFundAllocationRepository
{
    Task<(IEnumerable<FundAllocation> Items, int TotalCount)> GetPagedAsync(FundAllocationQueryParameters parameters);
    Task<IEnumerable<FundAllocation>> GetAllAsync();
    Task<FundAllocation?> GetByIdAsync(string id);
    Task<FundAllocation> AddAsync(FundAllocation allocation);
    Task<FundAllocation> UpdateAsync(FundAllocation allocation);
    Task<bool> DeleteAsync(string id);
    Task<int> BulkUpdateApprovalAsync(IEnumerable<string> ids, string approval);
    Task<FundAllocationSummaryMetrics> GetSummaryMetricsAsync();
    Task<string> GenerateNextIdAsync();
}
