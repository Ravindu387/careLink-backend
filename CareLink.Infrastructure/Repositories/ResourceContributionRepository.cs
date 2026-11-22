using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class ResourceContributionRepository : IResourceContributionRepository
{
    private readonly CareLinkDbContext _context;

    public ResourceContributionRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ResourceContribution> Items, int TotalCount)> GetPagedAsync(
        ResourceContributionQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(parameters);
        var totalCount = await query.CountAsync(cancellationToken);

        var page = parameters.Page > 0 ? parameters.Page : 1;
        var pageSize = parameters.PageSize > 0 ? parameters.PageSize : 10;

        var items = await query
            .OrderByDescending(c => c.ReceivedDate)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<ResourceContribution>> GetAllAsync(
        ResourceContributionQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(parameters);
        return await query
            .OrderByDescending(c => c.ReceivedDate)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ResourceContribution?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceContributions
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<ResourceContribution> AddAsync(ResourceContribution contribution, CancellationToken cancellationToken = default)
    {
        await _context.ResourceContributions.AddAsync(contribution, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return contribution;
    }

    public async Task<bool> UpdateAsync(ResourceContribution contribution, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ResourceContributions
            .FirstOrDefaultAsync(c => c.Id == contribution.Id, cancellationToken);

        if (existing == null) return false;

        if (!ReferenceEquals(existing, contribution))
        {
            _context.Entry(existing).CurrentValues.SetValues(contribution);
        }

        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateVerificationAsync(string id, string verification, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ResourceContributions
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (existing == null) return false;

        existing.Verification = verification;
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAllocationAsync(string id, string allocation, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ResourceContributions
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (existing == null) return false;

        existing.Allocation = allocation;
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> BulkVerifyAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        if (!idList.Any()) return 0;

        var items = await _context.ResourceContributions
            .Where(c => idList.Contains(c.Id))
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.Verification = "Verified";
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return items.Count;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ResourceContributions
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (existing == null) return false;

        _context.ResourceContributions.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<ResourceContribution> BuildQuery(ResourceContributionQueryParameters parameters)
    {
        var query = _context.ResourceContributions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var term = parameters.Search.Trim().ToLower();
            query = query.Where(c =>
                c.Id.ToLower().Contains(term) ||
                c.Donor.ToLower().Contains(term) ||
                c.Type.ToLower().Contains(term) ||
                c.Quantity.ToLower().Contains(term) ||
                c.Campaign.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Type) &&
            !parameters.Type.Equals("All types", StringComparison.OrdinalIgnoreCase) &&
            !parameters.Type.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var type = parameters.Type.Trim().ToLower();
            query = query.Where(c => c.Type.ToLower() == type);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Verification) &&
            !parameters.Verification.Equals("All verification states", StringComparison.OrdinalIgnoreCase) &&
            !parameters.Verification.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var ver = parameters.Verification.Trim().ToLower();
            query = query.Where(c => c.Verification.ToLower() == ver);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            !parameters.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase) &&
            !parameters.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var status = parameters.Status.Trim().ToLower();
            query = query.Where(c => c.Allocation.ToLower() == status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Campaign) &&
            !parameters.Campaign.Equals("All campaigns", StringComparison.OrdinalIgnoreCase) &&
            !parameters.Campaign.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var camp = parameters.Campaign.Trim().ToLower();
            query = query.Where(c => c.Campaign.ToLower() == camp);
        }

        if (parameters.ReceivedDate.HasValue)
        {
            var date = parameters.ReceivedDate.Value.Date;
            query = query.Where(c => c.ReceivedDate.Date == date);
        }

        if (!string.IsNullOrWhiteSpace(parameters.DateRange) &&
            !parameters.DateRange.Equals("Date range", StringComparison.OrdinalIgnoreCase))
        {
            var now = DateTime.UtcNow;
            if (parameters.DateRange.Equals("Last 7 days", StringComparison.OrdinalIgnoreCase))
            {
                var cutoff = now.AddDays(-7);
                query = query.Where(c => c.ReceivedDate >= cutoff);
            }
            else if (parameters.DateRange.Equals("Last 30 days", StringComparison.OrdinalIgnoreCase))
            {
                var cutoff = now.AddDays(-30);
                query = query.Where(c => c.ReceivedDate >= cutoff);
            }
            else if (parameters.DateRange.Equals("Last 90 days", StringComparison.OrdinalIgnoreCase))
            {
                var cutoff = now.AddDays(-90);
                query = query.Where(c => c.ReceivedDate >= cutoff);
            }
        }

        return query;
    }
}
