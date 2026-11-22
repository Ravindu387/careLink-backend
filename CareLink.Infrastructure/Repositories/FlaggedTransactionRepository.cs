using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class FlaggedTransactionRepository : IFlaggedTransactionRepository
{
    private readonly CareLinkDbContext _context;

    public FlaggedTransactionRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<FlaggedTransaction> Items, int TotalCount)> GetPagedAsync(FlaggedTransactionQueryParameters parameters)
    {
        var query = _context.FlaggedTransactions.AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.ToLower();
            query = query.Where(f =>
                f.Id.ToLower().Contains(search) ||
                f.Record.ToLower().Contains(search) ||
                f.Reason.ToLower().Contains(search) ||
                f.Party.ToLower().Contains(search));
        }

        // Record type filter
        if (!string.IsNullOrWhiteSpace(parameters.RecordType) && parameters.RecordType != "All record types")
        {
            var prefix = parameters.RecordType switch
            {
                "Donations" => "DON",
                "Expenditure" => "EXP",
                "Allocations" => "ALL",
                _ => ""
            };
            if (!string.IsNullOrEmpty(prefix))
                query = query.Where(f => f.Record.StartsWith(prefix));
        }

        // Severity filter
        if (!string.IsNullOrWhiteSpace(parameters.Severity) && parameters.Severity != "All severities")
            query = query.Where(f => f.Severity == parameters.Severity);

        // Status filter
        if (!string.IsNullOrWhiteSpace(parameters.Status) && parameters.Status != "All statuses")
            query = query.Where(f => f.Status == parameters.Status);

        var totalCount = await query.CountAsync();

        // Sort
        query = parameters.DateSort switch
        {
            "Oldest first" => query.OrderBy(f => f.FlaggedAt),
            _ => query.OrderByDescending(f => f.FlaggedAt)
        };

        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<FlaggedTransaction>> GetAllAsync()
    {
        return await _context.FlaggedTransactions
            .OrderByDescending(f => f.FlaggedAt)
            .ToListAsync();
    }

    public async Task<FlaggedTransaction?> GetByIdAsync(string id)
    {
        return await _context.FlaggedTransactions.FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<FlaggedTransaction> AddAsync(FlaggedTransaction flag)
    {
        await _context.FlaggedTransactions.AddAsync(flag);
        await _context.SaveChangesAsync();
        return flag;
    }

    public async Task<FlaggedTransaction> UpdateAsync(FlaggedTransaction flag)
    {
        flag.UpdatedAt = DateTime.UtcNow;
        _context.FlaggedTransactions.Update(flag);
        await _context.SaveChangesAsync();
        return flag;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var flag = await _context.FlaggedTransactions.FirstOrDefaultAsync(f => f.Id == id);
        if (flag == null) return false;
        _context.FlaggedTransactions.Remove(flag);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<FlaggedTransactionSummaryMetrics> GetSummaryMetricsAsync()
    {
        var all = await _context.FlaggedTransactions.ToListAsync();

        var openStatuses = new[] { "Investigation Required", "Under Review", "Deferred" };
        var highPriorityFlags = all.Where(f => f.Severity == "High" && f.Status != "Resolved").ToList();

        return new FlaggedTransactionSummaryMetrics
        {
            TotalCount = all.Count,
            OpenFlags = all.Count(f => openStatuses.Contains(f.Status)),
            HighPriority = highPriorityFlags.Count,
            UnderReview = all.Count(f => f.Status == "Under Review"),
            Resolved = all.Count(f => f.Status == "Resolved"),
            HighPriorityCombinedAmount = highPriorityFlags.Sum(f => f.Amount)
        };
    }

    public async Task<string> GenerateNextIdAsync()
    {
        var ids = await _context.FlaggedTransactions.Select(f => f.Id).ToListAsync();
        var maxNum = ids
            .Select(id =>
            {
                var parts = id.Split('-');
                return parts.Length == 2 && int.TryParse(parts[1], out var num) ? num : 0;
            })
            .DefaultIfEmpty(200)
            .Max();
        return $"FLG-{maxNum + 1}";
    }
}
