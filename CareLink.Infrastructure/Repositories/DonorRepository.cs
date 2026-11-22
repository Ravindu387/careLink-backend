using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class DonorRepository : IDonorRepository
{
    private readonly CareLinkDbContext _context;

    public DonorRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Donor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Donors
            .AsNoTracking()
            .OrderByDescending(d => d.Contributions)
            .ThenBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Donor> Items, int TotalCount)> GetPagedAsync(
        DonorQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Donors.AsNoTracking().AsQueryable();

        // Search text
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLower();
            query = query.Where(d =>
                d.Id.ToLower().Contains(search) ||
                d.Name.ToLower().Contains(search) ||
                d.Contact.ToLower().Contains(search) ||
                d.Type.ToLower().Contains(search) ||
                d.Scope.ToLower().Contains(search) ||
                (d.Notes != null && d.Notes.ToLower().Contains(search)));
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

        // Status filter
        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            !parameters.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            var status = parameters.Status.Trim().ToLower();
            query = query.Where(d => d.Status.ToLower() == status);
        }

        // Scope filter
        if (!string.IsNullOrWhiteSpace(parameters.Scope) &&
            !parameters.Scope.Equals("All scopes", StringComparison.OrdinalIgnoreCase))
        {
            var scope = parameters.Scope.Trim().ToLower();
            query = query.Where(d => d.Scope.ToLower() == scope);
        }

        // Contributions range
        if (parameters.MinContribution.HasValue)
        {
            query = query.Where(d => d.Contributions >= parameters.MinContribution.Value);
        }

        if (parameters.MaxContribution.HasValue)
        {
            query = query.Where(d => d.Contributions <= parameters.MaxContribution.Value);
        }

        // Date range filter based on CreatedAt
        if (!string.IsNullOrWhiteSpace(parameters.DateRange) &&
            !parameters.DateRange.Equals("Date range", StringComparison.OrdinalIgnoreCase))
        {
            var now = DateTime.UtcNow.Date;
            var range = parameters.DateRange.Trim().ToLower();
            if (range == "last 7 days")
            {
                var cutoff = now.AddDays(-7);
                query = query.Where(d => d.CreatedAt >= cutoff);
            }
            else if (range == "last 30 days")
            {
                var cutoff = now.AddDays(-30);
                query = query.Where(d => d.CreatedAt >= cutoff);
            }
            else if (range == "last 90 days")
            {
                var cutoff = now.AddDays(-90);
                query = query.Where(d => d.CreatedAt >= cutoff);
            }
            else if (range == "this year")
            {
                var cutoff = new DateTime(now.Year, 1, 1);
                query = query.Where(d => d.CreatedAt >= cutoff);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting & pagination
        var page = parameters.Page.GetValueOrDefault(1);
        if (page < 1) page = 1;
        var pageSize = parameters.PageSize.GetValueOrDefault(10);
        if (pageSize < 1) pageSize = 10;

        var items = await query
            .OrderByDescending(d => d.Contributions)
            .ThenByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Donor?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Donors
            .FirstOrDefaultAsync(d => d.Id.ToLower() == id.Trim().ToLower(), cancellationToken);
    }

    public async Task<Donor> AddAsync(Donor donor, CancellationToken cancellationToken = default)
    {
        await _context.Donors.AddAsync(donor, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return donor;
    }

    public async Task<Donor?> UpdateAsync(Donor donor, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Donors
            .FirstOrDefaultAsync(d => d.Id.ToLower() == donor.Id.Trim().ToLower(), cancellationToken);

        if (existing == null) return null;

        existing.Name = donor.Name;
        existing.Type = donor.Type;
        existing.Contact = donor.Contact;
        existing.Scope = donor.Scope;
        existing.Verification = donor.Verification;
        existing.Contributions = donor.Contributions;
        existing.Status = donor.Status;
        existing.Address = donor.Address;
        existing.Notes = donor.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Donors
            .FirstOrDefaultAsync(d => d.Id.ToLower() == id.Trim().ToLower(), cancellationToken);

        if (existing == null) return false;

        _context.Donors.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> BulkUpdateStatusAsync(
        IEnumerable<string> ids,
        string status,
        CancellationToken cancellationToken = default)
    {
        var normalizedIds = ids.Select(i => i.Trim().ToLower()).ToList();
        var records = await _context.Donors
            .Where(d => normalizedIds.Contains(d.Id.ToLower()))
            .ToListAsync(cancellationToken);

        if (!records.Any()) return 0;

        foreach (var donor in records)
        {
            donor.Status = status;
            donor.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return records.Count;
    }

    public async Task<DonorSummaryMetrics> GetSummaryMetricsAsync(CancellationToken cancellationToken = default)
    {
        var donors = await _context.Donors.AsNoTracking().ToListAsync(cancellationToken);

        return new DonorSummaryMetrics
        {
            TotalDonors = donors.Count,
            ActiveDonors = donors.Count(d => d.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)),
            VerifiedDonors = donors.Count(d => d.Verification.Equals("Verified", StringComparison.OrdinalIgnoreCase)),
            UnderReviewDonors = donors.Count(d => d.Verification.Equals("Under Review", StringComparison.OrdinalIgnoreCase)),
            RestrictedDonors = donors.Count(d => d.Status.Equals("Restricted", StringComparison.OrdinalIgnoreCase)),
            TotalContributions = donors.Sum(d => d.Contributions)
        };
    }
}
