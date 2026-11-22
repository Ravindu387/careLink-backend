using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class DonationRepository : IDonationRepository
{
    private readonly CareLinkDbContext _context;

    public DonationRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DonationRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DonationRecords
            .AsNoTracking()
            .OrderByDescending(d => d.ReceivedDate)
            .ThenByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<DonationRecord> Items, int TotalCount)> GetPagedAsync(
        DonationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DonationRecords.AsNoTracking().AsQueryable();

        // Search text
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLower();
            query = query.Where(d =>
                d.Id.ToLower().Contains(search) ||
                d.Donor.ToLower().Contains(search) ||
                d.Campaign.ToLower().Contains(search) ||
                d.Amount.ToLower().Contains(search) ||
                (d.Notes != null && d.Notes.ToLower().Contains(search)));
        }

        // Tab filter
        if (!string.IsNullOrWhiteSpace(parameters.Tab) &&
            !parameters.Tab.Equals("All records", StringComparison.OrdinalIgnoreCase))
        {
            var tab = parameters.Tab.Trim().ToLower();
            if (tab == "pending verification")
            {
                query = query.Where(d => d.Verification == "Pending");
            }
            else if (tab == "monetary")
            {
                query = query.Where(d => d.Type == "Monetary");
            }
            else if (tab == "material")
            {
                query = query.Where(d => d.Type == "Material");
            }
            else if (tab == "flagged")
            {
                query = query.Where(d => d.Verification == "Flagged");
            }
        }

        // Type filter
        if (!string.IsNullOrWhiteSpace(parameters.Type) &&
            !parameters.Type.Equals("All types", StringComparison.OrdinalIgnoreCase))
        {
            var type = parameters.Type.Trim().ToLower();
            query = query.Where(d => d.Type.ToLower() == type);
        }

        // Verification filter
        if (!string.IsNullOrWhiteSpace(parameters.Verification) &&
            !parameters.Verification.Equals("All verification states", StringComparison.OrdinalIgnoreCase))
        {
            var verification = parameters.Verification.Trim().ToLower();
            query = query.Where(d => d.Verification.ToLower() == verification);
        }

        // Allocation filter
        if (!string.IsNullOrWhiteSpace(parameters.Allocation) &&
            !parameters.Allocation.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            var allocation = parameters.Allocation.Trim().ToLower();
            query = query.Where(d => d.Allocation.ToLower() == allocation);
        }

        // Campaign filter
        if (!string.IsNullOrWhiteSpace(parameters.Campaign) &&
            !parameters.Campaign.Equals("All campaigns", StringComparison.OrdinalIgnoreCase))
        {
            var campaign = parameters.Campaign.Trim().ToLower();
            query = query.Where(d => d.Campaign.ToLower() == campaign);
        }

        // Date range filter
        if (!string.IsNullOrWhiteSpace(parameters.DateRange) &&
            !parameters.DateRange.Equals("Date range", StringComparison.OrdinalIgnoreCase))
        {
            var now = DateTime.UtcNow.Date;
            var range = parameters.DateRange.Trim().ToLower();
            if (range == "last 7 days")
            {
                var cutoff = now.AddDays(-7);
                query = query.Where(d => d.ReceivedDate >= cutoff);
            }
            else if (range == "last 30 days")
            {
                var cutoff = now.AddDays(-30);
                query = query.Where(d => d.ReceivedDate >= cutoff);
            }
            else if (range == "last 90 days")
            {
                var cutoff = now.AddDays(-90);
                query = query.Where(d => d.ReceivedDate >= cutoff);
            }
            else if (range == "this year")
            {
                var cutoff = new DateTime(now.Year, 1, 1);
                query = query.Where(d => d.ReceivedDate >= cutoff);
            }
        }

        // Custom date limits
        if (parameters.StartDate.HasValue)
        {
            var start = parameters.StartDate.Value.Date;
            query = query.Where(d => d.ReceivedDate >= start);
        }

        if (parameters.EndDate.HasValue)
        {
            var end = parameters.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(d => d.ReceivedDate <= end);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting & pagination
        var page = parameters.Page.GetValueOrDefault(1);
        if (page < 1) page = 1;
        var pageSize = parameters.PageSize.GetValueOrDefault(10);
        if (pageSize < 1) pageSize = 10;

        var items = await query
            .OrderByDescending(d => d.ReceivedDate)
            .ThenByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<DonationRecord?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.DonationRecords
            .FirstOrDefaultAsync(d => d.Id.ToLower() == id.Trim().ToLower(), cancellationToken);
    }

    public async Task<DonationRecord> AddAsync(DonationRecord donation, CancellationToken cancellationToken = default)
    {
        await _context.DonationRecords.AddAsync(donation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return donation;
    }

    public async Task<DonationRecord?> UpdateAsync(DonationRecord donation, CancellationToken cancellationToken = default)
    {
        var existing = await _context.DonationRecords
            .FirstOrDefaultAsync(d => d.Id.ToLower() == donation.Id.Trim().ToLower(), cancellationToken);

        if (existing == null) return null;

        existing.Donor = donation.Donor;
        existing.Type = donation.Type;
        existing.Amount = donation.Amount;
        existing.NumericAmount = donation.NumericAmount;
        existing.Campaign = donation.Campaign;
        existing.Received = donation.Received;
        existing.ReceivedDate = donation.ReceivedDate;
        existing.Verification = donation.Verification;
        existing.Allocation = donation.Allocation;
        existing.Notes = donation.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.DonationRecords
            .FirstOrDefaultAsync(d => d.Id.ToLower() == id.Trim().ToLower(), cancellationToken);

        if (existing == null) return false;

        _context.DonationRecords.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> BulkUpdateAsync(
        IEnumerable<string> ids,
        string? verification,
        string? allocation,
        CancellationToken cancellationToken = default)
    {
        var normalizedIds = ids.Select(i => i.Trim().ToLower()).ToList();
        var records = await _context.DonationRecords
            .Where(d => normalizedIds.Contains(d.Id.ToLower()))
            .ToListAsync(cancellationToken);

        if (!records.Any()) return 0;

        foreach (var record in records)
        {
            if (!string.IsNullOrWhiteSpace(verification))
            {
                record.Verification = verification;
            }
            if (!string.IsNullOrWhiteSpace(allocation))
            {
                record.Allocation = allocation;
            }
            record.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return records.Count;
    }

    public async Task<DonationSummaryMetrics> GetSummaryMetricsAsync(CancellationToken cancellationToken = default)
    {
        var records = await _context.DonationRecords.AsNoTracking().ToListAsync(cancellationToken);

        var totalRecords = records.Count;
        var pendingVerification = records.Count(d => d.Verification.Equals("Pending", StringComparison.OrdinalIgnoreCase));
        var monetaryCount = records.Count(d => d.Type.Equals("Monetary", StringComparison.OrdinalIgnoreCase));
        var materialCount = records.Count(d => d.Type.Equals("Material", StringComparison.OrdinalIgnoreCase));
        var flaggedCount = records.Count(d => d.Verification.Equals("Flagged", StringComparison.OrdinalIgnoreCase));
        var allocatedCount = records.Count(d => d.Allocation.Equals("Allocated", StringComparison.OrdinalIgnoreCase));

        var totalMonetaryValue = records
            .Where(d => d.Type.Equals("Monetary", StringComparison.OrdinalIgnoreCase) && d.NumericAmount.HasValue)
            .Sum(d => d.NumericAmount!.Value);

        return new DonationSummaryMetrics
        {
            TotalRecords = totalRecords,
            PendingVerificationCount = pendingVerification,
            MonetaryCount = monetaryCount,
            MaterialCount = materialCount,
            FlaggedCount = flaggedCount,
            TotalMonetaryValue = totalMonetaryValue,
            AllocatedCount = allocatedCount
        };
    }
}
