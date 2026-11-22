using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryLiveOperationsMapRepository : ILiveOperationsMapRepository
{
    private readonly ConcurrentDictionary<string, OperationPoint> _points = new();

    public InMemoryLiveOperationsMapRepository()
    {
        SeedDefaultData();
    }

    public Task<IReadOnlyList<OperationPoint>> GetAllAsync(
        OperationPointFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _points.Values.AsEnumerable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(p =>
                    p.RequestId.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    p.Title.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    p.Location.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    p.District.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    p.AssignedTeam.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    (p.Notes != null && p.Notes.Contains(s, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(filter.District) &&
                !filter.District.Equals("All districts", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.District.Equals(filter.District.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Layer) &&
                !filter.Layer.Equals("All operational layers", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Type.Equals(filter.Layer.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Priority) &&
                !filter.Priority.Equals("Any priority", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Priority.Equals(filter.Priority.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                !filter.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Status.Equals(filter.Status.Trim(), StringComparison.OrdinalIgnoreCase));
            }
        }

        var list = query
            .OrderByDescending(p => p.Priority == "Critical")
            .ThenByDescending(p => p.Priority == "High")
            .ThenByDescending(p => p.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<OperationPoint>>(list);
    }

    public Task<OperationPoint?> GetByIdAsync(
        string idOrRequestId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idOrRequestId))
            return Task.FromResult<OperationPoint?>(null);

        var trimmed = idOrRequestId.Trim();

        // Check by direct ID
        if (_points.TryGetValue(trimmed, out var found))
            return Task.FromResult<OperationPoint?>(found);

        // Check by RequestId
        var byRequest = _points.Values.FirstOrDefault(p =>
            p.RequestId.Equals(trimmed, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(byRequest);
    }

    public Task<OperationPoint> AddAsync(
        OperationPoint point,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(point.Id))
        {
            point.Id = Guid.NewGuid().ToString("N")[..8];
        }

        _points[point.Id] = point;
        return Task.FromResult(point);
    }

    public Task<OperationPoint?> UpdateAsync(
        OperationPoint point,
        CancellationToken cancellationToken = default)
    {
        if (!_points.ContainsKey(point.Id))
            return Task.FromResult<OperationPoint?>(null);

        _points[point.Id] = point;
        return Task.FromResult<OperationPoint?>(point);
    }

    public Task<OperationPoint?> AssignTeamAsync(
        string id,
        string assignedTeam,
        string? status = null,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var existing = _points.Values.FirstOrDefault(p =>
            p.Id.Equals(id, StringComparison.OrdinalIgnoreCase) ||
            p.RequestId.Equals(id, StringComparison.OrdinalIgnoreCase));

        if (existing == null) return Task.FromResult<OperationPoint?>(null);

        existing.AssignedTeam = assignedTeam;
        if (!string.IsNullOrWhiteSpace(status)) existing.Status = status;
        if (!string.IsNullOrWhiteSpace(notes)) existing.Notes = notes;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        _points[existing.Id] = existing;

        return Task.FromResult<OperationPoint?>(existing);
    }

    public Task<OperationPoint?> UpdateCoordinatesAsync(
        string id,
        double x,
        double y,
        double? latitude = null,
        double? longitude = null,
        CancellationToken cancellationToken = default)
    {
        var existing = _points.Values.FirstOrDefault(p =>
            p.Id.Equals(id, StringComparison.OrdinalIgnoreCase) ||
            p.RequestId.Equals(id, StringComparison.OrdinalIgnoreCase));

        if (existing == null) return Task.FromResult<OperationPoint?>(null);

        existing.X = x;
        existing.Y = y;
        if (latitude.HasValue) existing.Latitude = latitude.Value;
        if (longitude.HasValue) existing.Longitude = longitude.Value;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        _points[existing.Id] = existing;

        return Task.FromResult<OperationPoint?>(existing);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Task.FromResult(false);

        // Try direct key
        if (_points.TryRemove(id, out _))
            return Task.FromResult(true);

        // Try matching request ID
        var match = _points.Values.FirstOrDefault(p =>
            p.RequestId.Equals(id.Trim(), StringComparison.OrdinalIgnoreCase));

        if (match != null && _points.TryRemove(match.Id, out _))
            return Task.FromResult(true);

        return Task.FromResult(false);
    }

    public Task<LiveOperationsMapMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var all = _points.Values.ToList();

        var districts = all.Select(p => p.District)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(d => d)
            .ToList();

        var layers = all.Select(p => p.Type)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        var metrics = new LiveOperationsMapMetrics
        {
            TotalPoints = all.Count,
            RequestCount = all.Count(p => p.Type.Equals("Request", StringComparison.OrdinalIgnoreCase)),
            ShelterCount = all.Count(p => p.Type.Equals("Shelter", StringComparison.OrdinalIgnoreCase)),
            WarehouseCount = all.Count(p => p.Type.Equals("Warehouse", StringComparison.OrdinalIgnoreCase)),
            TeamCount = all.Count(p => p.Type.Equals("Team", StringComparison.OrdinalIgnoreCase)),
            CriticalCount = all.Count(p => p.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase)),
            HighCount = all.Count(p => p.Priority.Equals("High", StringComparison.OrdinalIgnoreCase)),
            MediumCount = all.Count(p => p.Priority.Equals("Medium", StringComparison.OrdinalIgnoreCase)),
            TotalPeopleAffected = all.Sum(p => p.People),
            Districts = districts,
            Layers = layers,
            LastSyncedAt = DateTimeOffset.UtcNow
        };

        return Task.FromResult(metrics);
    }

    public Task ResetDefaultDataAsync(CancellationToken cancellationToken = default)
    {
        _points.Clear();
        SeedDefaultData();
        return Task.CompletedTask;
    }

    private void SeedDefaultData()
    {
        var seedList = new List<OperationPoint>
        {
            new()
            {
                Id = "1",
                RequestId = "REQ-2841",
                Title = "Emergency shelter",
                District = "North Ward",
                Location = "Riverside, North Ward",
                Type = "Request",
                Priority = "Critical",
                People = 6,
                Confidence = 94,
                NearestShelter = "1.4 km",
                AssignedTeam = "Unassigned",
                Status = "Active",
                Time = "18 minutes ago",
                X = 27,
                Y = 58,
                ContactName = "Rohan Fernando",
                ContactPhone = "+94 77 234 5671",
                Notes = "Families stranded due to rapid river level rise."
            },
            new()
            {
                Id = "2",
                RequestId = "REQ-2839",
                Title = "Medical supplies",
                District = "Central Ward",
                Location = "Old Town, Central Ward",
                Type = "Request",
                Priority = "High",
                People = 12,
                Confidence = 91,
                NearestShelter = "2.1 km",
                AssignedTeam = "Medical Team 3",
                Status = "Dispatched",
                Time = "26 minutes ago",
                X = 48,
                Y = 42,
                ContactName = "Dr. K. Silva",
                ContactPhone = "+94 71 890 1234",
                Notes = "Insulin and emergency dressing kits urgently required."
            },
            new()
            {
                Id = "3",
                RequestId = "SHEL-038",
                Title = "Riverton High School",
                District = "Central Ward",
                Location = "Central Ward",
                Type = "Shelter",
                Priority = "Medium",
                People = 84,
                Confidence = 96,
                NearestShelter = "Current location",
                AssignedTeam = "Shelter Team 1",
                Status = "Operational",
                Time = "Updated recently",
                X = 72,
                Y = 34,
                Capacity = 200,
                CurrentOccupancy = 84,
                SuppliesStatus = "Adequate food and bedding for 3 days",
                ContactName = "Principal D. Alwis",
                ContactPhone = "+94 11 254 7890"
            },
            new()
            {
                Id = "4",
                RequestId = "WH-012",
                Title = "Central Warehouse",
                District = "Eastbank",
                Location = "Eastbank Logistics Hub",
                Type = "Warehouse",
                Priority = "Medium",
                People = 0,
                Confidence = 98,
                NearestShelter = "3.2 km",
                AssignedTeam = "Logistics Team",
                Status = "Operational",
                Time = "Updated recently",
                X = 82,
                Y = 72,
                Capacity = 5000,
                SuppliesStatus = "8,500 food ration packs, 12,000 bottled water units",
                ContactName = "Major H. Senaratne",
                ContactPhone = "+94 11 445 6789"
            },
            new()
            {
                Id = "5",
                RequestId = "TEAM-024",
                Title = "Response Team 24",
                District = "Central Ward",
                Location = "Central Ward Base",
                Type = "Team",
                Priority = "High",
                People = 8,
                Confidence = 90,
                NearestShelter = "2.0 km",
                AssignedTeam = "Rescue Unit 24",
                Status = "Dispatched",
                Time = "5 minutes ago",
                X = 53,
                Y = 78,
                ContactName = "Capt. Nuwan Gunawardena",
                ContactPhone = "+94 77 334 5566",
                Notes = "Equipped with high-clearance 4x4 and medical response gear."
            },
            new()
            {
                Id = "6",
                RequestId = "REQ-2834",
                Title = "Food & water rations",
                District = "North Ward",
                Location = "North Bank Road, North Ward",
                Type = "Request",
                Priority = "High",
                People = 28,
                Confidence = 89,
                NearestShelter = "1.9 km",
                AssignedTeam = "Unassigned",
                Status = "Active",
                Time = "34 minutes ago",
                X = 35,
                Y = 22,
                ContactName = "M. Perera",
                ContactPhone = "+94 70 554 9912",
                Notes = "Ground floor flooded; 7 families on upper floor terrace."
            },
            new()
            {
                Id = "7",
                RequestId = "REQ-2821",
                Title = "Elderly evacuation support",
                District = "West District",
                Location = "West River Bridge, West District",
                Type = "Request",
                Priority = "Critical",
                People = 4,
                Confidence = 98,
                NearestShelter = "0.9 km",
                AssignedTeam = "Swift Water Rescue Unit",
                Status = "Dispatched",
                Time = "12 minutes ago",
                X = 22,
                Y = 82,
                ContactName = "Grace Wickramasinghe",
                ContactPhone = "+94 77 443 2190",
                Notes = "Bedridden elderly persons requiring stretcher lift."
            },
            new()
            {
                Id = "8",
                RequestId = "SHEL-042",
                Title = "Community Sports Complex",
                District = "North Ward",
                Location = "Riverside Avenue, North Ward",
                Type = "Shelter",
                Priority = "High",
                People = 140,
                Confidence = 92,
                NearestShelter = "Current location",
                AssignedTeam = "Shelter Support Unit 2",
                Status = "Operational",
                Time = "Updated recently",
                X = 19,
                Y = 36,
                Capacity = 250,
                CurrentOccupancy = 140,
                SuppliesStatus = "Drinking water resupply requested",
                ContactName = "Officer Saman",
                ContactPhone = "+94 77 665 4321"
            },
            new()
            {
                Id = "9",
                RequestId = "SHEL-049",
                Title = "Westview Community Hall",
                District = "West District",
                Location = "Station Road, West District",
                Type = "Shelter",
                Priority = "Medium",
                People = 62,
                Confidence = 90,
                NearestShelter = "Current location",
                AssignedTeam = "West Civil Defense Team",
                Status = "Operational",
                Time = "Updated recently",
                X = 38,
                Y = 76,
                Capacity = 150,
                CurrentOccupancy = 62,
                SuppliesStatus = "Power and hygiene kits stocked",
                ContactName = "C. Bandara",
                ContactPhone = "+94 76 998 7765"
            },
            new()
            {
                Id = "10",
                RequestId = "WH-015",
                Title = "North Regional Depot",
                District = "North Ward",
                Location = "Industrial Zone, North Ward",
                Type = "Warehouse",
                Priority = "Medium",
                People = 0,
                Confidence = 95,
                NearestShelter = "1.2 km",
                AssignedTeam = "North Logistics Unit",
                Status = "Operational",
                Time = "Updated recently",
                X = 15,
                Y = 20,
                Capacity = 3000,
                SuppliesStatus = "Emergency bedding, tarpaulins, water filters",
                ContactName = "Logistics Mgr. T. Cooray",
                ContactPhone = "+94 77 889 0012"
            },
            new()
            {
                Id = "11",
                RequestId = "TEAM-018",
                Title = "Rapid Medical Unit 1",
                District = "North Ward",
                Location = "North Ward Transit Post",
                Type = "Team",
                Priority = "Critical",
                People = 6,
                Confidence = 95,
                NearestShelter = "1.5 km",
                AssignedTeam = "Medical Team 3",
                Status = "In Transit",
                Time = "8 minutes ago",
                X = 30,
                Y = 48,
                ContactName = "Lead Paramedic F. Razik",
                ContactPhone = "+94 72 331 4455",
                Notes = "En route to REQ-2839 with triage kits."
            },
            new()
            {
                Id = "12",
                RequestId = "TEAM-031",
                Title = "Swift Water Rescue Unit",
                District = "West District",
                Location = "River Sector West",
                Type = "Team",
                Priority = "Critical",
                People = 10,
                Confidence = 94,
                NearestShelter = "1.0 km",
                AssignedTeam = "Swift Water Rescue Unit",
                Status = "Dispatched",
                Time = "15 minutes ago",
                X = 18,
                Y = 68,
                ContactName = "Lt. Commander D. Ratnayake",
                ContactPhone = "+94 77 990 1234",
                Notes = "Operating two motorized inflatable rescue craft."
            }
        };

        foreach (var p in seedList)
        {
            _points[p.Id] = p;
        }
    }
}
