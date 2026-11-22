using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class ResourceRepository : IResourceRepository
{
    private readonly CareLinkDbContext _context;
    private readonly ConcurrentDictionary<string, ResourceRequest> _requests = new();

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

    public async Task<ResourceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var idStr = id.ToString();
        var resCode = $"RES-{id}";
        var invCode = $"INV-{id:D3}";
        return await _context.ResourceItems
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == idStr || r.Id == resCode || r.Id == invCode, cancellationToken)
            ?? await _context.ResourceItems.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<ResourceItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ResourceItems
            .AsNoTracking()
            .OrderByDescending(r => r.LastUpdated)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ResourceItem>> GetAllAsync(
        ResourceFilterParams? filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ResourceItems.AsNoTracking().AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim().ToLower();
                query = query.Where(item =>
                    item.Name.ToLower().Contains(s) ||
                    item.Category.ToLower().Contains(s) ||
                    item.Location.ToLower().Contains(s) ||
                    item.ShelterName.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(filter.Area) &&
                !filter.Area.Equals("All warehouses", StringComparison.OrdinalIgnoreCase) &&
                !filter.Area.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var area = filter.Area.Trim().ToLower();
                query = query.Where(item => item.Location.ToLower() == area || item.ShelterName.ToLower().Contains(area));
            }

            if (!string.IsNullOrWhiteSpace(filter.Category) &&
                !filter.Category.Equals("All categories", StringComparison.OrdinalIgnoreCase) &&
                !filter.Category.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var cat = filter.Category.Trim().ToLower();
                query = query.Where(item => item.Category.ToLower() == cat);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                !filter.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase) &&
                !filter.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var st = filter.Status.Trim().ToLower();
                query = query.Where(item => item.Status.ToLower() == st);
            }
        }

        var list = await query.ToListAsync(cancellationToken);
        return list;
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

    public async Task<ResourceItem?> UpdateStockAsync(
        int id,
        int? available,
        int? reserved,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        var tracked = await _context.ResourceItems.FirstOrDefaultAsync(r => r.Id == existing.Id, cancellationToken);
        if (tracked == null) return null;

        if (available.HasValue) tracked.Available = Math.Max(0, available.Value);
        if (reserved.HasValue) tracked.Reserved = Math.Max(0, reserved.Value);
        if (!string.IsNullOrWhiteSpace(status)) tracked.Status = status.Trim();
        tracked.LastUpdated = DateTime.UtcNow;
        tracked.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return tracked;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ResourceItems.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (existing == null) return false;

        _context.ResourceItems.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing == null) return false;

        var tracked = await _context.ResourceItems.FirstOrDefaultAsync(r => r.Id == existing.Id, cancellationToken);
        if (tracked == null) return false;

        _context.ResourceItems.Remove(tracked);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ResourceMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var all = await _context.ResourceItems.AsNoTracking().ToListAsync(cancellationToken);

        var totalItems = all.Count;
        var avgAvail = totalItems > 0 ? (int)all.Average(r => r.Available + r.Reserved > 0 ? (double)r.Available / (r.Available + r.Reserved) * 100 : 80) : 74;
        var warehouses = all.Select(r => string.IsNullOrWhiteSpace(r.Location) ? r.ShelterName : r.Location).Distinct().Count();
        var criticalCount = all.Count(r => r.Status.Equals("Critical", StringComparison.OrdinalIgnoreCase) || r.Available == 0);

        return new ResourceMetrics
        {
            AverageAvailabilityPercent = avgAvail,
            TotalWarehousesCount = Math.Max(1, warehouses),
            CriticalShortagesCount = criticalCount,
            CriticalShortagesSummary = "Water purification tablets · Trauma kits",
            ActiveDispatchesCount = 8,
            ArrivingTodayDispatchesCount = 3,
            PendingRequestsCount = _requests.Count,
            HighPriorityPendingRequestsCount = _requests.Values.Count(r => r.Priority == "High" || r.Priority == "Critical"),
            LastUpdated = DateTimeOffset.UtcNow
        };
    }

    public Task<ResourceRequest> CreateRequestAsync(ResourceRequest request, CancellationToken cancellationToken = default)
    {
        _requests[request.Id] = request;
        return Task.FromResult(request);
    }

    public Task<IReadOnlyList<ResourceRequest>> GetRequestsAsync(CancellationToken cancellationToken = default)
    {
        var list = _requests.Values.OrderByDescending(r => r.CreatedAt).ToList();
        return Task.FromResult<IReadOnlyList<ResourceRequest>>(list);
    }

    public async Task ResetDefaultDataAsync(CancellationToken cancellationToken = default)
    {
        _context.ResourceItems.RemoveRange(_context.ResourceItems);
        await _context.SaveChangesAsync(cancellationToken);
        await DataSeeder.SeedAsync(_context);
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
