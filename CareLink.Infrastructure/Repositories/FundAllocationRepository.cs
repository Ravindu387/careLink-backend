using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class FundAllocationRepository : IFundAllocationRepository
{
    private readonly CareLinkDbContext _context;

    public FundAllocationRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<FundAllocation> Items, int TotalCount)> GetPagedAsync(FundAllocationQueryParameters parameters)
    {
        var query = _context.FundAllocations.AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.ToLower();
            query = query.Where(a =>
                a.Id.ToLower().Contains(search) ||
                a.Source.ToLower().Contains(search) ||
                a.Purpose.ToLower().Contains(search) ||
                a.Recipient.ToLower().Contains(search));
        }

        // Source / Type filter
        if (!string.IsNullOrWhiteSpace(parameters.Source) && parameters.Source != "All types")
        {
            query = query.Where(a => a.Source == parameters.Source);
        }

        // Approval filter
        if (!string.IsNullOrWhiteSpace(parameters.Approval) && parameters.Approval != "All verification states")
        {
            query = query.Where(a => a.Approval == parameters.Approval);
        }

        // Disbursement / Status filter
        if (!string.IsNullOrWhiteSpace(parameters.Disbursement) && parameters.Disbursement != "All statuses")
        {
            query = query.Where(a => a.Disbursement == parameters.Disbursement);
        }

        // Date range filter
        if (!string.IsNullOrWhiteSpace(parameters.DateRange) && parameters.DateRange != "Date range")
        {
            var cutoff = parameters.DateRange switch
            {
                "Last 7 days" => DateTime.UtcNow.AddDays(-7),
                "Last 30 days" => DateTime.UtcNow.AddDays(-30),
                "Last 90 days" => DateTime.UtcNow.AddDays(-90),
                _ => DateTime.MinValue
            };
            if (cutoff > DateTime.MinValue)
            {
                query = query.Where(a => a.AllocationDate >= cutoff);
            }
        }

        // Min/Max Amount filter
        if (parameters.MinAmount.HasValue)
        {
            query = query.Where(a => a.Amount >= parameters.MinAmount.Value);
        }
        if (parameters.MaxAmount.HasValue)
        {
            query = query.Where(a => a.Amount <= parameters.MaxAmount.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.AllocationDate)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<FundAllocation>> GetAllAsync()
    {
        return await _context.FundAllocations
            .OrderByDescending(a => a.AllocationDate)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<FundAllocation?> GetByIdAsync(string id)
    {
        return await _context.FundAllocations.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<FundAllocation> AddAsync(FundAllocation allocation)
    {
        await _context.FundAllocations.AddAsync(allocation);
        await _context.SaveChangesAsync();
        return allocation;
    }

    public async Task<FundAllocation> UpdateAsync(FundAllocation allocation)
    {
        allocation.UpdatedAt = DateTime.UtcNow;
        _context.FundAllocations.Update(allocation);
        await _context.SaveChangesAsync();
        return allocation;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var item = await _context.FundAllocations.FirstOrDefaultAsync(a => a.Id == id);
        if (item == null) return false;
        _context.FundAllocations.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> BulkUpdateApprovalAsync(IEnumerable<string> ids, string approval)
    {
        var idList = ids.ToList();
        var allocations = await _context.FundAllocations
            .Where(a => idList.Contains(a.Id))
            .ToListAsync();

        var count = 0;
        foreach (var item in allocations)
        {
            item.Approval = approval;
            item.UpdatedAt = DateTime.UtcNow;
            count++;
        }

        await _context.SaveChangesAsync();
        return count;
    }

    public async Task<FundAllocationSummaryMetrics> GetSummaryMetricsAsync()
    {
        var all = await _context.FundAllocations.ToListAsync();

        var bySource = all
            .GroupBy(a => a.Source)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.Amount));

        return new FundAllocationSummaryMetrics
        {
            TotalAllocated = all.Sum(a => a.Amount),
            TotalDisbursed = all.Where(a => a.Disbursement == "Disbursed" || a.Disbursement.Contains("Disbursed")).Sum(a => a.Amount),
            TotalPending = all.Where(a => a.Disbursement == "Pending").Sum(a => a.Amount),
            ApprovedCount = all.Count(a => a.Approval == "Approved"),
            UnderReviewCount = all.Count(a => a.Approval == "Under Review"),
            RejectedCount = all.Count(a => a.Approval == "Rejected"),
            DisbursedCount = all.Count(a => a.Disbursement == "Disbursed"),
            TotalCount = all.Count,
            BySource = bySource
        };
    }

    public async Task<string> GenerateNextIdAsync()
    {
        var existingIds = await _context.FundAllocations
            .Select(a => a.Id)
            .ToListAsync();

        var maxNum = existingIds
            .Select(id =>
            {
                var parts = id.Split('-');
                return parts.Length == 2 && int.TryParse(parts[1], out var num) ? num : 0;
            })
            .DefaultIfEmpty(4400)
            .Max();

        return $"ALL-{maxNum + 1}";
    }
}
