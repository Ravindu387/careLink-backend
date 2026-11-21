using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class ResourceRepository : IResourceRepository
{
    private readonly CareLinkDbContext _context;

    public ResourceRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<ResourceItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceItems
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ResourceItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ResourceItems
            .AsNoTracking()
            .OrderByDescending(r => r.LastUpdated)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<ResourceItem> Items, int TotalCount)> GetPagedAsync(
        ResourceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ResourceItems.AsNoTracking().AsQueryable();

        // Search text
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLower();
            query = query.Where(r =>
                r.Id.ToLower().Contains(search) ||
                r.Name.ToLower().Contains(search) ||
                r.Category.ToLower().Contains(search));
        }

        // Category filter
        if (!string.IsNullOrWhiteSpace(parameters.Category) &&
            !parameters.Category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
        {
            var cat = parameters.Category.Trim().ToLower();
            query = query.Where(r => r.Category.ToLower() == cat);
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            !parameters.Status.Equals("All stock statuses", StringComparison.OrdinalIgnoreCase))
        {
            var status = parameters.Status.Trim().ToLower();
            if (status == "healthy")
            {
                query = query.Where(r => r.Available >= r.Minimum);
            }
            else if (status == "low stock")
            {
                query = query.Where(r => r.Available > 0 && r.Available < r.Minimum && r.Available >= (int)(r.Minimum * 0.5));
            }
            else if (status == "critical")
            {
                query = query.Where(r => r.Available > 0 && r.Available < (int)(r.Minimum * 0.5));
            }
            else if (status == "out of stock")
            {
                query = query.Where(r => r.Available == 0);
            }
        }

        // Expiry filter
        if (!string.IsNullOrWhiteSpace(parameters.ExpiryFilter) &&
            !parameters.ExpiryFilter.Equals("Any expiry", StringComparison.OrdinalIgnoreCase))
        {
            var exp = parameters.ExpiryFilter.Trim().ToLower();
            var today = DateTime.UtcNow.Date;
            if (exp.Contains("has expiry"))
            {
                query = query.Where(r => r.ExpiryDate.HasValue);
            }
            else if (exp.Contains("no expiry"))
            {
                query = query.Where(r => !r.ExpiryDate.HasValue);
            }
            else if (exp.Contains("soon"))
            {
                var next30 = today.AddDays(30);
                query = query.Where(r => r.ExpiryDate.HasValue && r.ExpiryDate.Value >= today && r.ExpiryDate.Value <= next30);
            }
        }

        // Shelter filter
        if (!string.IsNullOrWhiteSpace(parameters.ShelterId) &&
            !parameters.ShelterId.Equals("All shelters", StringComparison.OrdinalIgnoreCase))
        {
            var shelter = parameters.ShelterId.Trim().ToLower();
            query = query.Where(r =>
                r.ShelterId.ToLower() == shelter ||
                r.ShelterName.ToLower().Contains(shelter));
        }

        // Quantity filters
        if (parameters.MinQuantity.HasValue)
        {
            query = query.Where(r => r.Available >= parameters.MinQuantity.Value);
        }

        if (parameters.MaxQuantity.HasValue)
        {
            query = query.Where(r => r.Available <= parameters.MaxQuantity.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = parameters.Page > 0 ? parameters.Page : 1;
        var pageSize = parameters.PageSize > 0 ? parameters.PageSize : 10;

        var items = await query
            .OrderByDescending(r => r.LastUpdated)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(int HealthyCount, int LowStockCount, int CriticalCount, int OutOfStockCount, int ImmediateActionCount, int ExpiringSoonCount, int TotalItemsCount)> GetSummaryMetricsAsync(CancellationToken cancellationToken = default)
    {
        var all = await _context.ResourceItems.AsNoTracking().ToListAsync(cancellationToken);

        int healthy = 0;
        int lowStock = 0;
        int critical = 0;
        int outOfStock = 0;

        foreach (var r in all)
        {
            if (r.Available == 0) outOfStock++;
            else if (r.Available < r.Minimum * 0.5) critical++;
            else if (r.Available < r.Minimum) lowStock++;
            else healthy++;
        }

        int immediateAction = critical + outOfStock;

        var today = DateTime.UtcNow.Date;
        var next30 = today.AddDays(30);

        int expiringSoon = all.Count(r =>
            r.ExpiryDate.HasValue &&
            r.ExpiryDate.Value.Date >= today &&
            r.ExpiryDate.Value.Date <= next30);

        return (
            healthy,
            lowStock,
            critical,
            outOfStock,
            immediateAction,
            expiringSoon,
            all.Count
        );
    }

    public async Task<ResourceItem> AddAsync(ResourceItem item, CancellationToken cancellationToken = default)
    {
        await _context.ResourceItems.AddAsync(item, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<ResourceItem?> UpdateAsync(ResourceItem item, CancellationToken cancellationToken = default)
    {
        _context.ResourceItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ResourceItems.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (existing == null) return false;

        _context.ResourceItems.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ResourceDistribution> AddDistributionAsync(ResourceDistribution distribution, CancellationToken cancellationToken = default)
    {
        await _context.ResourceDistributions.AddAsync(distribution, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return distribution;
    }

    public async Task<IEnumerable<ResourceDistribution>> GetDistributionsAsync(string? resourceId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ResourceDistributions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(resourceId))
        {
            query = query.Where(d => d.ResourceId == resourceId);
        }

        return await query
            .OrderByDescending(d => d.DistributedAt)
            .ToListAsync(cancellationToken);
    }
}
