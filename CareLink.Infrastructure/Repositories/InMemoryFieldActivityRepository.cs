using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryFieldActivityRepository : IFieldActivityRepository
{
    private readonly ConcurrentDictionary<string, FieldActivity> _activities = new();

    public InMemoryFieldActivityRepository()
    {
        SeedDefaultData();
    }

    public Task<IReadOnlyList<FieldActivity>> GetAllAsync(
        FieldActivityFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _activities.Values.AsEnumerable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(a =>
                    a.Id.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    a.Name.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    a.Location.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    a.Team.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    a.Target.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    a.Sector.Contains(s, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Tab) &&
                !filter.Tab.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var tab = filter.Tab.Trim();
                if (tab.Equals("Needs action", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(a =>
                        a.Status.Equals("Scheduled", StringComparison.OrdinalIgnoreCase) ||
                        a.Status.Equals("Overdue", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(a =>
                        a.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                        a.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(a => a.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Archived", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(a => a.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase));
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Sector) &&
                !filter.Sector.Equals("All sectors", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(a => a.Sector.Equals(filter.Sector.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Location) &&
                !filter.Location.Equals("All locations", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(a => a.Location.Contains(filter.Location.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Team) &&
                !filter.Team.Equals("All teams", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(a => a.Team.Equals(filter.Team.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                !filter.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(a => a.Status.Equals(filter.Status.Trim(), StringComparison.OrdinalIgnoreCase));
            }
        }

        var list = query
            .OrderByDescending(a => a.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<FieldActivity>>(list);
    }

    public Task<FieldActivity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _activities.TryGetValue(id, out var activity);
        return Task.FromResult(activity);
    }

    public Task<FieldActivity> AddAsync(FieldActivity activity, CancellationToken cancellationToken = default)
    {
        _activities[activity.Id] = activity;
        return Task.FromResult(activity);
    }

    public Task<FieldActivity?> UpdateAsync(FieldActivity activity, CancellationToken cancellationToken = default)
    {
        if (!_activities.ContainsKey(activity.Id))
        {
            return Task.FromResult<FieldActivity?>(null);
        }

        activity.UpdatedAt = DateTimeOffset.UtcNow;
        _activities[activity.Id] = activity;
        return Task.FromResult<FieldActivity?>(activity);
    }

    public Task<FieldActivity?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default)
    {
        if (!_activities.TryGetValue(id, out var activity))
        {
            return Task.FromResult<FieldActivity?>(null);
        }

        activity.Status = status.Trim();
        activity.UpdatedAt = DateTimeOffset.UtcNow;

        if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            activity.CompletedAt = DateTimeOffset.UtcNow;
        }

        _activities[id] = activity;
        return Task.FromResult<FieldActivity?>(activity);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_activities.TryRemove(id, out _));
    }

    public Task<FieldActivityMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var all = _activities.Values.ToList();

        var activeCount = all.Count(a =>
            a.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
            a.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase));

        var needsAction = all.Count(a =>
            a.Status.Equals("Scheduled", StringComparison.OrdinalIgnoreCase) ||
            a.Status.Equals("Overdue", StringComparison.OrdinalIgnoreCase));

        var completedCount = all.Count(a =>
            a.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));

        var archivedCount = all.Count(a =>
            a.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase));

        var metrics = new FieldActivityMetrics
        {
            ActiveToday = Math.Max(activeCount, 12),
            PeopleTargeted = 4840,
            PeopleReachedToday = 1864,
            TeamsDeployed = 18,
            StaffInField = 62,
            UpdatesOverdue = all.Count(a => a.Status.Equals("Overdue", StringComparison.OrdinalIgnoreCase)),
            TotalActivities = all.Count,
            NeedsActionCount = needsAction,
            ActiveCount = activeCount,
            CompletedCount = completedCount,
            ArchivedCount = archivedCount,
            LastUpdated = DateTimeOffset.UtcNow
        };

        return Task.FromResult(metrics);
    }

    public Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        _activities.Clear();
        SeedDefaultData();
        return Task.CompletedTask;
    }

    private void SeedDefaultData()
    {
        var now = DateTimeOffset.UtcNow;

        var seeds = new List<FieldActivity>
        {
            new()
            {
                Id = "ACT-1042",
                Name = "Household food distribution",
                Location = "North Ward - Zone 3",
                Scheduled = "22 Jun - 14:00",
                Team = "Field Team North",
                Target = "420 households",
                Sector = "Food Security",
                Status = "In Progress",
                Description = "Direct distribution of 7-day emergency food ration kits to flooded households.",
                CreatedAt = now.AddHours(-1),
                UpdatedAt = now.AddHours(-1)
            },
            new()
            {
                Id = "ACT-1038",
                Name = "Water quality monitoring",
                Location = "Eastbank - Sites 2–6",
                Scheduled = "22 Jun - 12:30",
                Team = "WASH Team 2",
                Target = "5 water points",
                Sector = "WASH",
                Status = "Completed",
                Description = "Rapid bacteriological water testing and chlorination dosage adjustments across five community bowser sites.",
                CreatedAt = now.AddHours(-3),
                UpdatedAt = now.AddHours(-1),
                CompletedAt = now.AddHours(-1)
            },
            new()
            {
                Id = "ACT-1034",
                Name = "Protection desk",
                Location = "Riverton High Shelter",
                Scheduled = "22 Jun - 09:00",
                Team = "Protection Team",
                Target = "86 consultations",
                Sector = "Protection",
                Status = "Active",
                Description = "Psychosocial first aid counseling and unaccompanied minor registration desk at main evacuation shelter.",
                CreatedAt = now.AddHours(-5),
                UpdatedAt = now.AddHours(-2)
            },
            new()
            {
                Id = "ACT-1029",
                Name = "Health awareness session",
                Location = "Eastbank Community Centre",
                Scheduled = "21 Jun - 16:00",
                Team = "Health Outreach",
                Target = "124 participants",
                Sector = "Health",
                Status = "Completed",
                Description = "Community hygiene and waterborne infection prevention education for community leaders.",
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now.AddHours(-8),
                CompletedAt = now.AddHours(-8)
            },
            new()
            {
                Id = "ACT-1025",
                Name = "Emergency shelter assessment",
                Location = "North Ward - Zone 2",
                Scheduled = "23 Jun - 10:00",
                Team = "Field Team North",
                Target = "60 households",
                Sector = "Shelter",
                Status = "Scheduled",
                Description = "Structural damage appraisal of semi-permanent residential dwellings along riverbank edge.",
                CreatedAt = now.AddHours(-10),
                UpdatedAt = now.AddHours(-10)
            },
            new()
            {
                Id = "ACT-1020",
                Name = "Community food outreach",
                Location = "South Ward",
                Scheduled = "23 Jun - 13:00",
                Team = "Food Support Team",
                Target = "200 families",
                Sector = "Food Security",
                Status = "Active",
                Description = "Hot meal packaging and dispatch for cutoff residents along secondary drainage canals.",
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now.AddHours(-4)
            },
            new()
            {
                Id = "ACT-1015",
                Name = "Shelter tarpaulin distribution",
                Location = "Riverton East",
                Scheduled = "24 Jun - 09:30",
                Team = "Field Team North",
                Target = "150 kits",
                Sector = "Shelter",
                Status = "Scheduled",
                Description = "Heavy duty weather proof tarpaulin and tie-down rope kit distribution.",
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddDays(-1)
            },
            new()
            {
                Id = "ACT-1010",
                Name = "Sanitation inspection",
                Location = "Central Ward Camps",
                Scheduled = "20 Jun - 11:00",
                Team = "WASH Team 2",
                Target = "12 camp latrines",
                Sector = "WASH",
                Status = "Archived",
                Description = "Initial intake sanitary inspection and pit latrine chemical treatment finished.",
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddDays(-2),
                CompletedAt = now.AddDays(-2)
            }
        };

        foreach (var a in seeds)
        {
            _activities[a.Id] = a;
        }
    }
}
