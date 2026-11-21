using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryCrisisEventRepository : ICrisisEventRepository
{
    private readonly ConcurrentDictionary<string, CrisisEvent> _events = new();
    private readonly ConcurrentDictionary<string, CrisisAccessRequest> _accessRequests = new();

    public InMemoryCrisisEventRepository()
    {
        SeedDefaultData();
    }

    public Task<IReadOnlyList<CrisisEvent>> GetAllAsync(
        CrisisEventFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _events.Values.AsEnumerable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(e =>
                    e.Id.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    e.Name.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    e.Area.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    e.Coordinator.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    (e.Description != null && e.Description.Contains(s, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Type) && !filter.Type.Equals("All crisis types", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(e => e.Type.Equals(filter.Type, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Severity) && !filter.Severity.Equals("All severity levels", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(e => e.Severity.StartsWith(filter.Severity, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) && !filter.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(e => e.Status.Equals(filter.Status, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.ShowArchived.HasValue)
            {
                if (filter.ShowArchived.Value)
                {
                    query = query.Where(e => e.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase) ||
                                             e.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    query = query.Where(e => !e.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase) &&
                                             !e.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase));
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.DateRange) && !filter.DateRange.Equals("Date range", StringComparison.OrdinalIgnoreCase))
            {
                if (filter.DateRange.Contains("2025"))
                {
                    query = query.Where(e => e.StartDate.Year == 2025);
                }
                if (filter.DateRange.Contains("Jun", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.StartDate.Month == 6);
                }
            }
        }

        var list = query
            .OrderByDescending(e => e.StartDate)
            .ToList();

        return Task.FromResult<IReadOnlyList<CrisisEvent>>(list);
    }

    public Task<CrisisEvent?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _events.TryGetValue(id, out var ev);
        return Task.FromResult(ev);
    }

    public Task<CrisisEvent> AddAsync(CrisisEvent crisisEvent, CancellationToken cancellationToken = default)
    {
        _events[crisisEvent.Id] = crisisEvent;
        return Task.FromResult(crisisEvent);
    }

    public Task<CrisisEvent?> UpdateAsync(CrisisEvent crisisEvent, CancellationToken cancellationToken = default)
    {
        if (!_events.ContainsKey(crisisEvent.Id))
        {
            return Task.FromResult<CrisisEvent?>(null);
        }

        _events[crisisEvent.Id] = crisisEvent;
        return Task.FromResult<CrisisEvent?>(crisisEvent);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var removed = _events.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<CrisisEventMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var items = _events.Values.ToList();

        var activeCount = items.Count(e => e.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                                           e.Status.Equals("Monitoring", StringComparison.OrdinalIgnoreCase));

        var majorCount = items.Count(e => e.Severity.StartsWith("Major", StringComparison.OrdinalIgnoreCase) ||
                                          e.SeverityLevel >= 4);

        var monitoringCount = items.Count(e => e.Status.Equals("Monitoring", StringComparison.OrdinalIgnoreCase));
        var resolvedCount = items.Count(e => e.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase));

        var totalAffected = items.Sum(e => e.AffectedPeopleCount);
        string formattedAffected = totalAffected >= 1000
            ? $"{totalAffected / 1000.0:0.#}K"
            : totalAffected.ToString();

        var coordinatorsCount = items
            .Where(e => !string.IsNullOrWhiteSpace(e.Coordinator))
            .Select(e => e.Coordinator)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        // Baseline total active coordinators across all operations
        var totalCoordinators = Math.Max(12, coordinatorsCount);

        var metrics = new CrisisEventMetrics
        {
            ActiveEvents = activeCount,
            MajorSeverityEvents = majorCount,
            TotalAffectedPeople = totalAffected,
            FormattedAffectedPeople = formattedAffected,
            AssignedCoordinators = totalCoordinators,
            TotalEvents = items.Count,
            MonitoringEvents = monitoringCount,
            ResolvedEvents = resolvedCount,
            DistrictsCount = 6
        };

        return Task.FromResult(metrics);
    }

    public Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        _events.Clear();
        _accessRequests.Clear();
        SeedDefaultData();
        return Task.CompletedTask;
    }

    public Task<CrisisAccessRequest> AddAccessRequestAsync(CrisisAccessRequest request, CancellationToken cancellationToken = default)
    {
        _accessRequests[request.Id] = request;
        return Task.FromResult(request);
    }

    public Task<IReadOnlyList<CrisisAccessRequest>> GetAccessRequestsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CrisisAccessRequest> list = _accessRequests.Values.OrderByDescending(r => r.SubmittedAt).ToList();
        return Task.FromResult(list);
    }

    private void SeedDefaultData()
    {
        var events = new List<CrisisEvent>
        {
            new()
            {
                Id = "CR-2025-018",
                Name = "Monsoon Floods 2025",
                Type = "Flood",
                Severity = "Level 4 - Major",
                SeverityLevel = 4,
                Area = "Greater Riverton",
                StartDate = new DateTimeOffset(2025, 6, 18, 8, 30, 0, TimeSpan.Zero),
                Sectors = "Food · WASH · Shelter",
                Status = "Active",
                RequestCount = 1284,
                Coordinator = "Amina Mensah",
                Description = "Extensive monsoon flooding along the Riverton basin affecting low-lying residential sectors.",
                AffectedPeopleCount = 142000,
                DistrictsCovered = 3
            },
            new()
            {
                Id = "CR-2025-014",
                Name = "North Ridge Landslide",
                Type = "Landslide",
                Severity = "Level 3 - Severe",
                SeverityLevel = 3,
                Area = "North District",
                StartDate = new DateTimeOffset(2025, 6, 12, 14, 15, 0, TimeSpan.Zero),
                Sectors = "Shelter · Protection",
                Status = "Monitoring",
                RequestCount = 218,
                Coordinator = "Joseph Okafor",
                Description = "Slope failure along North Ridge Highway causing localized evacuations and road closures.",
                AffectedPeopleCount = 18000,
                DistrictsCovered = 1
            },
            new()
            {
                Id = "CR-2025-009",
                Name = "Dengue Outbreak",
                Type = "Disease",
                Severity = "Level 2 - Moderate",
                SeverityLevel = 2,
                Area = "Eastbank",
                StartDate = new DateTimeOffset(2025, 5, 28, 9, 0, 0, TimeSpan.Zero),
                Sectors = "Community Health",
                Status = "Active",
                RequestCount = 486,
                Coordinator = "Dr. M. Rao",
                Description = "Seasonal mosquito-borne infection surge across high-density urban sectors in Eastbank.",
                AffectedPeopleCount = 21500,
                DistrictsCovered = 2
            },
            new()
            {
                Id = "CR-2025-004",
                Name = "Coastal Storm Response",
                Type = "Storm",
                Severity = "Level 1 - Low",
                SeverityLevel = 1,
                Area = "West Coast",
                StartDate = new DateTimeOffset(2025, 4, 14, 11, 45, 0, TimeSpan.Zero),
                Sectors = "Food Assistance",
                Status = "Completed",
                RequestCount = 92,
                Coordinator = "Nadia Silva",
                Description = "Post-tropical coastal depression with localized power disruption, now fully restored.",
                AffectedPeopleCount = 2500,
                DistrictsCovered = 1
            },
            new()
            {
                Id = "CR-2025-021",
                Name = "Riverbank Flooding",
                Type = "Flood",
                Severity = "Level 3 - Severe",
                SeverityLevel = 3,
                Area = "South District",
                StartDate = new DateTimeOffset(2025, 6, 22, 10, 0, 0, TimeSpan.Zero),
                Sectors = "Food · Shelter",
                Status = "Active",
                RequestCount = 312,
                Coordinator = "Amina Mensah",
                Description = "Secondary river overflow impacting agricultural settlements and rural transport routes.",
                AffectedPeopleCount = 14500,
                DistrictsCovered = 1
            },
            new()
            {
                Id = "CR-2025-022",
                Name = "Community Health Emergency",
                Type = "Disease",
                Severity = "Level 2 - Moderate",
                SeverityLevel = 2,
                Area = "East District",
                StartDate = new DateTimeOffset(2025, 6, 25, 12, 0, 0, TimeSpan.Zero),
                Sectors = "Community Health",
                Status = "Monitoring",
                RequestCount = 175,
                Coordinator = "Dr. M. Rao",
                Description = "Localized waterborne infection monitoring following initial monsoon flood subsidence.",
                AffectedPeopleCount = 8200,
                DistrictsCovered = 1
            }
        };

        foreach (var ev in events)
        {
            _events[ev.Id] = ev;
        }

        var sampleRequest = new CrisisAccessRequest
        {
            Id = "REQ-CR-1001",
            CrisisId = "CR-2025-018",
            EventName = "Monsoon Floods 2025",
            PartnerName = "Global Relief Network",
            Reason = "Field deployment of emergency food distribution team in Sector 3.",
            Status = "Approved",
            SubmittedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };
        _accessRequests[sampleRequest.Id] = sampleRequest;
    }
}
