using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class ExpenditureRepository : IExpenditureRepository
{
    private readonly CareLinkDbContext _context;

    public ExpenditureRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Expenditure> Items, int TotalCount)> GetPagedAsync(ExpenditureQueryParameters parameters)
    {
        var query = _context.Expenditures.AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.ToLower();
            query = query.Where(e =>
                e.Id.ToLower().Contains(search) ||
                e.Description.ToLower().Contains(search) ||
                e.Recipient.ToLower().Contains(search) ||
                e.Invoice.ToLower().Contains(search));
        }

        // Type filter
        if (!string.IsNullOrWhiteSpace(parameters.Type) && parameters.Type != "All types")
            query = query.Where(e => e.Type == parameters.Type);

        // Verification filter
        if (!string.IsNullOrWhiteSpace(parameters.Verification) && parameters.Verification != "All verification states")
            query = query.Where(e => e.Verification == parameters.Verification);

        // Payment status filter
        if (!string.IsNullOrWhiteSpace(parameters.PaymentStatus) && parameters.PaymentStatus != "All statuses")
            query = query.Where(e => e.PaymentStatus == parameters.PaymentStatus);

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
                query = query.Where(e => e.ExpenseDate >= cutoff);
        }

        // Amount range filter
        if (parameters.MinAmount.HasValue)
            query = query.Where(e => e.Amount >= parameters.MinAmount.Value);

        if (parameters.MaxAmount.HasValue)
            query = query.Where(e => e.Amount <= parameters.MaxAmount.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.ExpenseDate)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Expenditure>> GetAllAsync()
    {
        return await _context.Expenditures
            .OrderByDescending(e => e.ExpenseDate)
            .ThenByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<Expenditure?> GetByIdAsync(string id)
    {
        return await _context.Expenditures.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Expenditure> AddAsync(Expenditure expenditure)
    {
        await _context.Expenditures.AddAsync(expenditure);
        await _context.SaveChangesAsync();
        return expenditure;
    }

    public async Task<Expenditure> UpdateAsync(Expenditure expenditure)
    {
        expenditure.UpdatedAt = DateTime.UtcNow;
        _context.Expenditures.Update(expenditure);
        await _context.SaveChangesAsync();
        return expenditure;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var expenditure = await _context.Expenditures.FirstOrDefaultAsync(e => e.Id == id);
        if (expenditure == null) return false;
        _context.Expenditures.Remove(expenditure);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> BulkUpdateAsync(IEnumerable<string> ids, string action)
    {
        var idList = ids.ToList();
        var expenditures = await _context.Expenditures
            .Where(e => idList.Contains(e.Id))
            .ToListAsync();

        var count = 0;
        foreach (var exp in expenditures)
        {
            switch (action)
            {
                case "Approve":
                    exp.Verification = "Verified";
                    exp.PaymentStatus = "Pending";
                    count++;
                    break;
                case "Reject":
                    exp.Verification = "Rejected";
                    exp.PaymentStatus = "On Hold";
                    count++;
                    break;
                case "Pay":
                    if (exp.Verification == "Verified")
                    {
                        exp.PaymentStatus = "Paid";
                        count++;
                    }
                    break;
                case "Hold":
                    exp.PaymentStatus = "On Hold";
                    count++;
                    break;
            }
            exp.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return count;
    }

    public async Task<ExpenditureSummaryMetrics> GetSummaryMetricsAsync()
    {
        var all = await _context.Expenditures.ToListAsync();

        var byType = all
            .GroupBy(e => e.Type)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        return new ExpenditureSummaryMetrics
        {
            TotalExpenditure = all.Sum(e => e.Amount),
            TotalPaid = all.Where(e => e.PaymentStatus == "Paid").Sum(e => e.Amount),
            TotalPending = all.Where(e => e.PaymentStatus == "Pending").Sum(e => e.Amount),
            TotalOnHold = all.Where(e => e.PaymentStatus == "On Hold").Sum(e => e.Amount),
            PendingReviewCount = all.Count(e => e.Verification == "Pending Review"),
            VerifiedCount = all.Count(e => e.Verification == "Verified"),
            RejectedCount = all.Count(e => e.Verification == "Rejected"),
            PaidCount = all.Count(e => e.PaymentStatus == "Paid"),
            OnHoldCount = all.Count(e => e.PaymentStatus == "On Hold"),
            TotalCount = all.Count,
            ByType = byType
        };
    }

    public async Task<string> GenerateNextIdAsync()
    {
        var existingIds = await _context.Expenditures
            .Select(e => e.Id)
            .ToListAsync();

        var maxNum = existingIds
            .Select(id =>
            {
                var parts = id.Split('-');
                return parts.Length == 2 && int.TryParse(parts[1], out var num) ? num : 0;
            })
            .DefaultIfEmpty(9000)
            .Max();

        return $"EXP-{maxNum + 1}";
    }
}
