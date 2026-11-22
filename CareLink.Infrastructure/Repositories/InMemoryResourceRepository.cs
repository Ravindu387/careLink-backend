using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryResourceRepository : IResourceRepository
{
    private readonly ConcurrentDictionary<string, ResourceItem> _resources = new();
    private readonly ConcurrentDictionary<string, ResourceRequest> _requests = new();
    private readonly ConcurrentDictionary<string, ResourceDistribution> _distributions = new();

    public InMemoryResourceRepository()
    {
        SeedDefaultData();
    }

    public Task<IReadOnlyList<ResourceItem>> GetAllAsync(
        ResourceFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _resources.Values.AsEnumerable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(r =>
                    r.Name.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.Category.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.Location.Contains(s, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Area) &&
                !filter.Area.Equals("All operational areas", StringComparison.OrdinalIgnoreCase))
            {
                var a = filter.Area.Trim();
                query = query.Where(r => r.Location.Equals(a, StringComparison.OrdinalIgnoreCase) ||
                                         r.Location.Contains(a, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                !filter.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                var st = filter.Status.Trim();
                query = query.Where(r => r.Status.Equals(st, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Availability) &&
                !filter.Availability.Equals("Any availability", StringComparison.OrdinalIgnoreCase))
            {
                if (filter.Availability.Equals("Above 60%", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Availability > 60);
                }
                else if (filter.Availability.Equals("Below 60%", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Availability <= 60);
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Category) &&
                !filter.Category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Category.Equals(filter.Category.Trim(), StringComparison.OrdinalIgnoreCase));
            }
        }

        var list = query
            .OrderBy(r => r.Id)
            .ToList();

        return Task.FromResult<IReadOnlyList<ResourceItem>>(list);
    }

    public Task<IEnumerable<ResourceItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = _resources.Values.OrderBy(r => r.Id).ToList();
        return Task.FromResult<IEnumerable<ResourceItem>>(list);
    }

    public Task<ResourceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var idStr = id.ToString();
        var code = $"RES-{id:D3}";
        if (_resources.TryGetValue(code, out var item)) return Task.FromResult<ResourceItem?>(item);
        if (_resources.TryGetValue(idStr, out item)) return Task.FromResult<ResourceItem?>(item);
        item = _resources.Values.FirstOrDefault(r => r.Id == idStr || r.Id == code || (int.TryParse(r.Id.Replace("RES-", ""), out var parsed) && parsed == id));
        return Task.FromResult(item);
    }

    public Task<ResourceItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return Task.FromResult<ResourceItem?>(null);
        var trimmed = id.Trim();
        if (_resources.TryGetValue(trimmed, out var item)) return Task.FromResult<ResourceItem?>(item);
        item = _resources.Values.FirstOrDefault(r => r.Id.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        if (item == null && int.TryParse(trimmed, out var intId))
        {
            return GetByIdAsync(intId, cancellationToken);
        }
        return Task.FromResult(item);
    }

    public Task<ResourceItem> AddAsync(ResourceItem item, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
        {
            int maxId = 0;
            foreach (var r in _resources.Values)
            {
                var clean = r.Id.Replace("RES-", "");
                if (int.TryParse(clean, out var parsed) && parsed > maxId) maxId = parsed;
            }
            item.Id = $"RES-{maxId + 1:D3}";
        }
        item.UpdatedAt = DateTimeOffset.UtcNow;
        item.LastUpdated = DateTime.UtcNow;
        _resources[item.Id] = item;
        return Task.FromResult(item);
    }

    public Task<ResourceItem?> UpdateAsync(ResourceItem item, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(item.Id)) return Task.FromResult<ResourceItem?>(null);
        if (!_resources.ContainsKey(item.Id))
        {
            var existing = _resources.Values.FirstOrDefault(r => r.Id.Equals(item.Id, StringComparison.OrdinalIgnoreCase));
            if (existing == null) return Task.FromResult<ResourceItem?>(null);
            _resources.TryRemove(existing.Id, out _);
        }

        item.UpdatedAt = DateTimeOffset.UtcNow;
        item.LastUpdated = DateTime.UtcNow;
        _resources[item.Id] = item;
        return Task.FromResult<ResourceItem?>(item);
    }

    public Task<ResourceItem?> UpdateStockAsync(
        int id,
        int? available,
        int? reserved,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var item = _resources.Values.FirstOrDefault(r => r.Id == id.ToString() || r.Id == $"RES-{id:D3}" || (int.TryParse(r.Id.Replace("RES-", ""), out var parsed) && parsed == id));
        if (item == null)
        {
            return Task.FromResult<ResourceItem?>(null);
        }

        if (available.HasValue) item.Available = available.Value;
        if (reserved.HasValue) item.Reserved = reserved.Value;

        var total = item.Available + item.Reserved;
        item.Availability = total > 0
            ? (int)Math.Round((double)item.Available / total * 100)
            : 100;

        if (!string.IsNullOrWhiteSpace(status))
        {
            item.Status = status.Trim();
        }

        item.UpdatedAt = DateTimeOffset.UtcNow;
        item.LastUpdated = DateTime.UtcNow;
        return Task.FromResult<ResourceItem?>(item);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var idStr = id.ToString();
        var code = $"RES-{id:D3}";
        if (_resources.TryRemove(code, out _) || _resources.TryRemove(idStr, out _)) return Task.FromResult(true);
        var existing = _resources.Values.FirstOrDefault(r => r.Id == idStr || r.Id == code || (int.TryParse(r.Id.Replace("RES-", ""), out var parsed) && parsed == id));
        if (existing != null)
        {
            return Task.FromResult(_resources.TryRemove(existing.Id, out _));
        }
        return Task.FromResult(false);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(false);
        var trimmed = id.Trim();
        if (_resources.TryRemove(trimmed, out _)) return Task.FromResult(true);
        var existing = _resources.Values.FirstOrDefault(r => r.Id.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            return Task.FromResult(_resources.TryRemove(existing.Id, out _));
        }
        return Task.FromResult(false);
    }

    public Task<ResourceMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var items = _resources.Values.ToList();
        var avgAvailability = items.Count > 0
            ? (int)Math.Round(items.Average(r => (double)r.Availability))
            : 68;

        var criticalCount = items.Count(r => r.Status.Equals("Critical", StringComparison.OrdinalIgnoreCase));
        var highPendingCount = _requests.Values.Count(r => r.Priority.Equals("High", StringComparison.OrdinalIgnoreCase) ||
                                                          r.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase));

        var metrics = new ResourceMetrics
        {
            AverageAvailabilityPercent = avgAvailability,
            TotalWarehousesCount = 12,
            CriticalShortagesCount = Math.Max(3, criticalCount),
            CriticalShortagesSummary = "Medical · transport",
            ActiveDispatchesCount = 27,
            ArrivingTodayDispatchesCount = 8,
            PendingRequestsCount = 43 + _requests.Count,
            HighPriorityPendingRequestsCount = 12 + highPendingCount,
            LastUpdated = DateTimeOffset.UtcNow,
        };

        return Task.FromResult(metrics);
    }

    public Task<ResourceRequest> CreateRequestAsync(ResourceRequest request, CancellationToken cancellationToken = default)
    {
        _requests[request.Id] = request;
        return Task.FromResult(request);
    }

    public Task<IReadOnlyList<ResourceRequest>> GetRequestsAsync(CancellationToken cancellationToken = default)
    {
        var list = _requests.Values
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
        return Task.FromResult<IReadOnlyList<ResourceRequest>>(list);
    }

    public Task ResetDefaultDataAsync(CancellationToken cancellationToken = default)
    {
        return ResetToDefaultAsync(cancellationToken);
    }

    public Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        _resources.Clear();
        _requests.Clear();
        _distributions.Clear();
        SeedDefaultData();
        return Task.CompletedTask;
    }

    public Task<(IEnumerable<ResourceItem> Items, int TotalCount)> GetPagedAsync(
        ResourceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _resources.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLowerInvariant();
            query = query.Where(r =>
                r.Name.ToLowerInvariant().Contains(search) ||
                r.Id.ToLowerInvariant().Contains(search) ||
                (r.ShelterName != null && r.ShelterName.ToLowerInvariant().Contains(search)) ||
                (r.Location != null && r.Location.ToLowerInvariant().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Category) &&
            !parameters.Category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.Category.Equals(parameters.Category.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            !parameters.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.Status.Equals(parameters.Status.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(parameters.ShelterId))
        {
            query = query.Where(r => r.ShelterId == parameters.ShelterId);
        }

        if (parameters.MinQuantity.HasValue)
        {
            query = query.Where(r => r.Available >= parameters.MinQuantity.Value);
        }

        if (parameters.MaxQuantity.HasValue)
        {
            query = query.Where(r => r.Available <= parameters.MaxQuantity.Value);
        }

        var list = query.OrderBy(r => r.Id).ToList();
        var totalCount = list.Count;
        var page = parameters.Page > 0 ? parameters.Page : 1;
        var pageSize = parameters.PageSize > 0 ? parameters.PageSize : 10;

        var items = list
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(((IEnumerable<ResourceItem>)items, totalCount));
    }

    public Task<(int HealthyCount, int LowStockCount, int CriticalCount, int OutOfStockCount, int ImmediateActionCount, int ExpiringSoonCount, int TotalItemsCount)> GetSummaryMetricsAsync(CancellationToken cancellationToken = default)
    {
        var all = _resources.Values.ToList();

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

        return Task.FromResult((
            healthy,
            lowStock,
            critical,
            outOfStock,
            immediateAction,
            expiringSoon,
            all.Count
        ));
    }

    public Task<ResourceDistribution> AddDistributionAsync(ResourceDistribution distribution, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(distribution.Id))
        {
            distribution.Id = Guid.NewGuid().ToString();
        }
        _distributions[distribution.Id] = distribution;
        return Task.FromResult(distribution);
    }

    public Task<IEnumerable<ResourceDistribution>> GetDistributionsAsync(string? resourceId = null, CancellationToken cancellationToken = default)
    {
        var query = _distributions.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(resourceId))
        {
            query = query.Where(d => d.ResourceId == resourceId);
        }
        var list = query.OrderByDescending(d => d.DistributedAt).ToList();
        return Task.FromResult<IEnumerable<ResourceDistribution>>(list);
    }

    private void SeedDefaultData()
    {
        var now = DateTimeOffset.UtcNow;

        var initial = new List<ResourceItem>
        {
            new()
            {
                Id = "RES-001",
                Name = "Water - 10L",
                Category = "Food & Water",
                Location = "Central Depot",
                Available = 8420,
                Reserved = 3100,
                Minimum = 2000,
                Availability = 68,
                Status = "Healthy",
                Unit = "Bottles",
                LastRestocked = now.AddDays(-2),
                UpdatedAt = now.AddHours(-3)
            },
            new()
            {
                Id = "RES-002",
                Name = "Family Food Packs",
                Category = "Food & Water",
                Location = "North Hub",
                Available = 1284,
                Reserved = 940,
                Minimum = 1500,
                Availability = 42,
                Status = "Low Stock",
                Unit = "Packs",
                LastRestocked = now.AddDays(-4),
                UpdatedAt = now.AddHours(-5)
            },
            new()
            {
                Id = "RES-003",
                Name = "Shelter Kits",
                Category = "Accommodation",
                Location = "Eastbank Store",
                Available = 342,
                Reserved = 218,
                Minimum = 300,
                Availability = 61,
                Status = "Healthy",
                Unit = "Kits",
                LastRestocked = now.AddDays(-1),
                UpdatedAt = now.AddHours(-2)
            },
            new()
            {
                Id = "RES-004",
                Name = "Trauma Kits",
                Category = "Medical Supplies",
                Location = "Central Depot",
                Available = 84,
                Reserved = 160,
                Minimum = 200,
                Availability = 34,
                Status = "Critical",
                Unit = "Kits",
                LastRestocked = now.AddDays(-7),
                UpdatedAt = now.AddHours(-1)
            },
            new()
            {
                Id = "RES-005",
                Name = "Hygiene Packs",
                Category = "Health & Safety",
                Location = "West Hub",
                Available = 2410,
                Reserved = 850,
                Minimum = 1000,
                Availability = 74,
                Status = "Healthy",
                Unit = "Packs",
                LastRestocked = now.AddDays(-3),
                UpdatedAt = now.AddHours(-6)
            },
            new()
            {
                Id = "RES-006",
                Name = "Bedding & Blankets",
                Category = "Accommodation",
                Location = "North Hub",
                Available = 450,
                Reserved = 410,
                Minimum = 600,
                Availability = 52,
                Status = "Low Stock",
                Unit = "Sets",
                LastRestocked = now.AddDays(-5),
                UpdatedAt = now.AddHours(-4)
            },
            new()
            {
                Id = "RES-007",
                Name = "Baby Formula Packs",
                Category = "Food & Water",
                Location = "Central Depot",
                Available = 92,
                Reserved = 180,
                Minimum = 200,
                Availability = 33,
                Status = "Critical",
                Unit = "Packs",
                LastRestocked = now.AddDays(-6),
                UpdatedAt = now.AddMinutes(-45)
            },
            new()
            {
                Id = "RES-008",
                Name = "Generators (5kVA)",
                Category = "Logistics",
                Location = "Central Depot",
                Available = 14,
                Reserved = 12,
                Minimum = 10,
                Availability = 54,
                Status = "Healthy",
                Unit = "Units",
                LastRestocked = now.AddDays(-10),
                UpdatedAt = now.AddHours(-8)
            }
        };

        foreach (var item in initial)
        {
            _resources[item.Id] = item;
        }
    }
}
