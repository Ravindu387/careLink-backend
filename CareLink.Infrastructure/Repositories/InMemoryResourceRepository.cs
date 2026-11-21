using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryResourceRepository : IResourceRepository
{
    private readonly ConcurrentDictionary<int, ResourceItem> _resources = new();
    private readonly ConcurrentDictionary<string, ResourceRequest> _requests = new();

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

    public Task<ResourceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _resources.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }

    public Task<ResourceItem> AddAsync(ResourceItem item, CancellationToken cancellationToken = default)
    {
        _resources[item.Id] = item;
        return Task.FromResult(item);
    }

    public Task<ResourceItem?> UpdateAsync(ResourceItem item, CancellationToken cancellationToken = default)
    {
        if (!_resources.ContainsKey(item.Id))
        {
            return Task.FromResult<ResourceItem?>(null);
        }

        item.UpdatedAt = DateTimeOffset.UtcNow;
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
        if (!_resources.TryGetValue(id, out var item))
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
            item.Status = status;
        }
        else
        {
            item.Status = item.Availability > 60
                ? "Healthy"
                : item.Availability >= 30
                ? "Low Stock"
                : "Critical";
        }

        item.UpdatedAt = DateTimeOffset.UtcNow;
        _resources[id] = item;
        return Task.FromResult<ResourceItem?>(item);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var removed = _resources.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<ResourceMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var items = _resources.Values.ToList();

        var avgAvailability = items.Count > 0
            ? (int)Math.Round(items.Average(r => r.Availability))
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
        SeedDefaultData();
        return Task.CompletedTask;
    }

    private void SeedDefaultData()
    {
        var now = DateTimeOffset.UtcNow;

        var initial = new List<ResourceItem>
        {
            new()
            {
                Id = 1,
                Name = "Water - 10L",
                Category = "Food & Water",
                Location = "Central Depot",
                Available = 8420,
                Reserved = 3100,
                Availability = 68,
                Status = "Healthy",
                Unit = "Bottles",
                LastRestocked = now.AddDays(-2),
                UpdatedAt = now.AddHours(-3)
            },
            new()
            {
                Id = 2,
                Name = "Family Food Packs",
                Category = "Food & Water",
                Location = "North Hub",
                Available = 1284,
                Reserved = 940,
                Availability = 42,
                Status = "Low Stock",
                Unit = "Packs",
                LastRestocked = now.AddDays(-4),
                UpdatedAt = now.AddHours(-5)
            },
            new()
            {
                Id = 3,
                Name = "Shelter Kits",
                Category = "Accommodation",
                Location = "Eastbank Store",
                Available = 342,
                Reserved = 218,
                Availability = 61,
                Status = "Healthy",
                Unit = "Kits",
                LastRestocked = now.AddDays(-1),
                UpdatedAt = now.AddHours(-2)
            },
            new()
            {
                Id = 4,
                Name = "Trauma Kits",
                Category = "Medical",
                Location = "Central Depot",
                Available = 86,
                Reserved = 64,
                Availability = 25,
                Status = "Critical",
                Unit = "Kits",
                LastRestocked = now.AddDays(-6),
                UpdatedAt = now.AddHours(-1)
            },
            new()
            {
                Id = 5,
                Name = "Emergency Blankets",
                Category = "Accommodation",
                Location = "North Hub",
                Available = 760,
                Reserved = 240,
                Availability = 75,
                Status = "Healthy",
                Unit = "Units",
                LastRestocked = now.AddDays(-3),
                UpdatedAt = now.AddHours(-4)
            },
            new()
            {
                Id = 6,
                Name = "First Aid Kits",
                Category = "Medical",
                Location = "Eastbank Store",
                Available = 125,
                Reserved = 90,
                Availability = 35,
                Status = "Low Stock",
                Unit = "Kits",
                LastRestocked = now.AddDays(-5),
                UpdatedAt = now.AddHours(-6)
            },
            new()
            {
                Id = 7,
                Name = "Mobile Generators (5kW)",
                Category = "Logistics",
                Location = "Central Depot",
                Available = 24,
                Reserved = 18,
                Availability = 57,
                Status = "Low Stock",
                Unit = "Units",
                LastRestocked = now.AddDays(-7),
                UpdatedAt = now.AddHours(-12)
            },
            new()
            {
                Id = 8,
                Name = "Water Purification Tablets (10k)",
                Category = "Food & Water",
                Location = "North Hub",
                Available = 350,
                Reserved = 80,
                Availability = 81,
                Status = "Healthy",
                Unit = "Boxes",
                LastRestocked = now.AddDays(-1),
                UpdatedAt = now.AddHours(-8)
            }
        };

        foreach (var r in initial)
        {
            _resources[r.Id] = r;
        }
    }
}
