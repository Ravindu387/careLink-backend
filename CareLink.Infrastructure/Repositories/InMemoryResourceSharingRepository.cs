using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryResourceSharingRepository : IResourceSharingRepository
{
    private readonly ConcurrentDictionary<string, ResourceSharingCommitment> _commitments = new(StringComparer.OrdinalIgnoreCase);
    private int _idCounter = 1049;

    public InMemoryResourceSharingRepository()
    {
        SeedInitialData();
    }

    public Task<IReadOnlyList<ResourceSharingCommitment>> GetAllAsync(
        ResourceSharingQuery query,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<ResourceSharingCommitment> queryable = _commitments.Values;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLowerInvariant();
            queryable = queryable.Where(r =>
                r.Id.ToLowerInvariant().Contains(s) ||
                r.Resource.ToLowerInvariant().Contains(s) ||
                r.Source.ToLowerInvariant().Contains(s) ||
                r.Destination.ToLowerInvariant().Contains(s) ||
                r.Partner.ToLowerInvariant().Contains(s) ||
                r.Location.ToLowerInvariant().Contains(s) ||
                (r.Description != null && r.Description.ToLowerInvariant().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(query.Tab) && !query.Tab.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            queryable = query.Tab switch
            {
                "Needs action" => queryable.Where(r => r.Status == "Pending Approval" || r.Status == "Partial Delivery"),
                "Active" => queryable.Where(r => r.Status == "Approved" || r.Status == "In Transit"),
                "Completed" => queryable.Where(r => r.Status == "Delivered" || r.Status == "Completed"),
                "Archived" => queryable.Where(r => r.Status == "Archived"),
                _ => queryable
            };
        }

        if (!string.IsNullOrWhiteSpace(query.Type) && !query.Type.Equals("All resource types", StringComparison.OrdinalIgnoreCase))
        {
            queryable = queryable.Where(r => r.Type.Equals(query.Type, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Location) && !query.Location.Equals("All locations", StringComparison.OrdinalIgnoreCase))
        {
            queryable = queryable.Where(r => r.Location.Equals(query.Location, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Partner) && !query.Partner.Equals("All partners", StringComparison.OrdinalIgnoreCase))
        {
            queryable = queryable.Where(r => r.Partner.Equals(query.Partner, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            queryable = queryable.Where(r => r.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
        }

        IReadOnlyList<ResourceSharingCommitment> result = queryable
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<ResourceSharingCommitment?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _commitments.TryGetValue(id, out var commitment);
        return Task.FromResult(commitment);
    }

    public Task<ResourceSharingCommitment> CreateAsync(
        ResourceSharingCommitment commitment,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(commitment.Id))
        {
            commitment.Id = $"COM-{Interlocked.Increment(ref _idCounter)}";
        }

        commitment.CreatedAt = DateTimeOffset.UtcNow;
        commitment.UpdatedAt = DateTimeOffset.UtcNow;
        _commitments[commitment.Id] = commitment;

        return Task.FromResult(commitment);
    }

    public Task<ResourceSharingCommitment?> UpdateAsync(
        ResourceSharingCommitment commitment,
        CancellationToken cancellationToken = default)
    {
        if (!_commitments.ContainsKey(commitment.Id))
        {
            return Task.FromResult<ResourceSharingCommitment?>(null);
        }

        commitment.UpdatedAt = DateTimeOffset.UtcNow;
        _commitments[commitment.Id] = commitment;
        return Task.FromResult<ResourceSharingCommitment?>(commitment);
    }

    public Task<ResourceSharingCommitment?> UpdateStatusAsync(
        string id,
        string status,
        CancellationToken cancellationToken = default)
    {
        if (_commitments.TryGetValue(id, out var commitment))
        {
            commitment.Status = status;
            commitment.UpdatedAt = DateTimeOffset.UtcNow;
            return Task.FromResult<ResourceSharingCommitment?>(commitment);
        }

        return Task.FromResult<ResourceSharingCommitment?>(null);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_commitments.TryRemove(id, out _));
    }

    public Task<ResourceSharingMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var list = _commitments.Values.ToList();
        var total = list.Count;
        var needsAction = list.Count(r => r.Status == "Pending Approval" || r.Status == "Partial Delivery");
        var active = list.Count(r => r.Status == "Approved" || r.Status == "In Transit");
        var completed = list.Count(r => r.Status == "Delivered" || r.Status == "Completed");
        var archived = list.Count(r => r.Status == "Archived");
        var inDelivery = list.Count(r => r.Status == "In Transit");

        var metrics = new ResourceSharingMetrics
        {
            ActiveCommitments = active + needsAction,
            ActiveCommitmentsSubtitle = $"{inDelivery} in delivery",
            UnitsCommitted = "24.8K",
            UnitsCommittedSubtitle = "Across 8 categories",
            DeliveredThisWeek = "18.4K",
            DeliveredThisWeekSubtitle = "96% confirmed",
            Exceptions = list.Count(r => r.Status == "Partial Delivery"),
            ExceptionsSubtitle = "Partial or delayed",
            TotalCommitments = total,
            NeedsActionCount = needsAction,
            ActiveCount = active,
            CompletedCount = completed,
            ArchivedCount = archived,
            LastUpdated = DateTimeOffset.UtcNow
        };

        return Task.FromResult(metrics);
    }

    public Task ResetAsync(CancellationToken cancellationToken = default)
    {
        _commitments.Clear();
        _idCounter = 1049;
        SeedInitialData();
        return Task.CompletedTask;
    }

    private void SeedInitialData()
    {
        var items = new List<ResourceSharingCommitment>
        {
            new()
            {
                Id = "COM-1048",
                Resource = "Family Food Packs",
                Quantity = 500,
                Source = "North District Hub",
                Destination = "North Ward Shelters",
                Partner = "Global Relief Network",
                Scheduled = "22 Jun - 14:10",
                Status = "In Transit",
                Type = "Food",
                Location = "North District",
                Description = "Emergency family dry food rations with high-calorie biscuits and cooking essentials.",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
            },
            new()
            {
                Id = "COM-1044",
                Resource = "Hygiene Kits",
                Quantity = 300,
                Source = "Eastbank Store",
                Destination = "Eastbank Communities",
                Partner = "CareLink Network",
                Scheduled = "22 Jun - 13:48",
                Status = "Approved",
                Type = "Health",
                Location = "Eastbank",
                Description = "Essential personal hygiene kits containing soap, toothbrushes, towels, and sanitizer packs.",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-3)
            },
            new()
            {
                Id = "COM-1039",
                Resource = "Water Purification Tablets",
                Quantity = 20000,
                Source = "Central Hub",
                Destination = "North Ward",
                Partner = "Global Relief Network",
                Scheduled = "22 Jun - 12:14",
                Status = "Partial Delivery",
                Type = "WASH",
                Location = "North District",
                Description = "Bulk water disinfection tablets for distribution to flooded residential communities.",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-6)
            },
            new()
            {
                Id = "COM-1035",
                Resource = "Emergency Medical Kits",
                Quantity = 150,
                Source = "Medical Store",
                Destination = "Riverside Hospital",
                Partner = "Health Support Foundation",
                Scheduled = "21 Jun - 16:30",
                Status = "Delivered",
                Type = "Health",
                Location = "Greater Riverton",
                Description = "Trauma response bandages, antiseptic, burn dressings, and IV starter sets.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = "COM-1028",
                Resource = "Emergency Shelter Tents",
                Quantity = 80,
                Source = "South Logistics Hub",
                Destination = "Coastal Relief Camp",
                Partner = "Global Relief Network",
                Scheduled = "21 Jun - 11:20",
                Status = "Pending Approval",
                Type = "Shelter",
                Location = "West Coast",
                Description = "Weather-resistant 6-person family shelters with groundsheet and installation frame.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = "COM-1021",
                Resource = "Drinking Water Bottles",
                Quantity = 1000,
                Source = "Eastbank Store",
                Destination = "Eastbank Communities",
                Partner = "CareLink Network",
                Scheduled = "20 Jun - 09:15",
                Status = "Completed",
                Type = "WASH",
                Location = "Eastbank",
                Description = "Bottled clean mineral drinking water pallets for community evacuation point.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = "COM-1014",
                Resource = "Heavy Duty Water Pumps",
                Quantity = 5,
                Source = "Central Depot",
                Destination = "Sub-Sector 4",
                Partner = "Global Relief Network",
                Scheduled = "19 Jun - 08:30",
                Status = "Archived",
                Type = "WASH",
                Location = "North District",
                Description = "High-flow submersible dewatering pumps for residential basement clearance.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-4)
            }
        };

        foreach (var item in items)
        {
            _commitments[item.Id] = item;
        }
    }
}
