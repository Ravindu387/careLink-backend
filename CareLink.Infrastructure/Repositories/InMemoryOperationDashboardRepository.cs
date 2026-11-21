using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryOperationDashboardRepository : IOperationDashboardRepository
{
    private readonly object _lock = new();
    private OperationDashboardState _state = new();

    public InMemoryOperationDashboardRepository()
    {
        SeedDefaultData();
    }

    public Task<OperationDashboardState> GetDashboardStateAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(CloneState(_state));
        }
    }

    public Task<IReadOnlyList<DashboardKpi>> GetKpisAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<DashboardKpi>>(_state.Kpis.ToList());
        }
    }

    public Task<IReadOnlyList<WeeklyRequestTrend>> GetWeeklyTrendsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<WeeklyRequestTrend>>(_state.WeeklyTrends.ToList());
        }
    }

    public Task<IReadOnlyList<RecentOperationActivity>> GetRecentActivitiesAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var list = _state.RecentActivities
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .ToList();
            return Task.FromResult<IReadOnlyList<RecentOperationActivity>>(list);
        }
    }

    public Task<RecentOperationActivity> AddActivityAsync(RecentOperationActivity activity, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(activity.Id))
            {
                activity.Id = Guid.NewGuid().ToString("N")[..8];
            }

            _state.RecentActivities.Insert(0, activity);
            _state.LastSyncedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(activity);
        }
    }

    public Task<ActiveCrisisBriefing> GetActiveCrisisAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_state.Crisis);
        }
    }

    public Task<IReadOnlyList<UpcomingBriefing>> GetUpcomingBriefingsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<UpcomingBriefing>>(_state.UpcomingBriefings.ToList());
        }
    }

    public Task<RecentOperationActivity> LogQuickIncidentAsync(
        string title,
        string district,
        string location,
        string severity,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var incidentCode = $"INC-{new Random().Next(100, 999)}";
            var activity = new RecentOperationActivity
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Title = $"New {severity} Incident: {title}",
                Location = location,
                District = district,
                Time = "Just now",
                Status = "New",
                Timestamp = DateTimeOffset.UtcNow,
                ReferenceId = incidentCode
            };

            _state.RecentActivities.Insert(0, activity);

            // Increment Open Requests KPI if present
            var openReqKpi = _state.Kpis.FirstOrDefault(k => k.Title.Contains("Open requests", StringComparison.OrdinalIgnoreCase));
            if (openReqKpi != null)
            {
                openReqKpi.Detail = $"{new Random().Next(87, 95)} today";
            }

            _state.ActiveOperationalPointsCount++;
            _state.LastSyncedAt = DateTimeOffset.UtcNow;

            return Task.FromResult(activity);
        }
    }

    public Task ResetDefaultDataAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            SeedDefaultData();
            return Task.CompletedTask;
        }
    }

    private void SeedDefaultData()
    {
        var now = DateTimeOffset.UtcNow;

        _state = new OperationDashboardState
        {
            Crisis = new ActiveCrisisBriefing
            {
                Id = "CR-2025-001",
                Title = "Monsoon Floods 2025",
                Severity = "SEVERITY 4 - MAJOR",
                Level = 4,
                ActiveSince = "Active since 18 Jun · 4 districts affected",
                AffectedDistrictsCount = 4,
                LeadAgency = "National Disaster Authority",
                NextBriefing = "14:30 · in 42 min",
                NextBriefingTime = "14:30"
            },
            Kpis = new List<DashboardKpi>
            {
                new()
                {
                    Title = "Active crisis events",
                    Value = "3",
                    Detail = "1 severity level 4",
                    Icon = "AlertTriangle",
                    Color = "red",
                    Category = "Crisis"
                },
                new()
                {
                    Title = "Open requests",
                    Value = "1,284",
                    Detail = "86 today",
                    Icon = "Inbox",
                    Color = "blue",
                    Category = "Requests"
                },
                new()
                {
                    Title = "Critical priority",
                    Value = "47",
                    Detail = "12 overdue",
                    Icon = "TrendingUp",
                    Color = "red",
                    Category = "Requests"
                },
                new()
                {
                    Title = "Available resources",
                    Value = "68%",
                    Detail = "3 critical shortages",
                    Icon = "Package",
                    Color = "amber",
                    Category = "Resources"
                },
                new()
                {
                    Title = "Active shelters",
                    Value = "38",
                    Detail = "8,642 people",
                    Icon = "Home",
                    Color = "green",
                    Category = "Shelters"
                },
                new()
                {
                    Title = "Response teams",
                    Value = "126",
                    Detail = "84 currently deployed",
                    Icon = "Users",
                    Color = "blue",
                    Category = "Teams"
                },
                new()
                {
                    Title = "AI reviews pending",
                    Value = "8",
                    Detail = "3 critical cases",
                    Icon = "Sparkles",
                    Color = "purple",
                    Category = "AI"
                },
                new()
                {
                    Title = "Avg response time",
                    Value = "2h 18m",
                    Detail = "-18 min this week",
                    Icon = "Clock",
                    Color = "green",
                    Category = "Performance"
                }
            },
            WeeklyTrends = new List<WeeklyRequestTrend>
            {
                new() { Day = "Mon", Received = 45, Fulfilled = 32 },
                new() { Day = "Tue", Received = 68, Fulfilled = 48 },
                new() { Day = "Wed", Received = 52, Fulfilled = 39 },
                new() { Day = "Thu", Received = 82, Fulfilled = 61 },
                new() { Day = "Fri", Received = 65, Fulfilled = 50 },
                new() { Day = "Sat", Received = 95, Fulfilled = 73 },
                new() { Day = "Sun", Received = 72, Fulfilled = 58 }
            },
            RecentActivities = new List<RecentOperationActivity>
            {
                new()
                {
                    Id = "ACT-01",
                    Title = "Emergency shelter request received",
                    Location = "Riverside, North Ward",
                    District = "North Ward",
                    Time = "5 minutes ago",
                    Status = "New",
                    Timestamp = now.AddMinutes(-5),
                    ReferenceId = "REQ-2841"
                },
                new()
                {
                    Id = "ACT-02",
                    Title = "Response team deployed",
                    Location = "Old Town, Central Ward",
                    District = "Central Ward",
                    Time = "18 minutes ago",
                    Status = "In Progress",
                    Timestamp = now.AddMinutes(-18),
                    ReferenceId = "TEAM-024"
                },
                new()
                {
                    Id = "ACT-03",
                    Title = "Resource allocation completed",
                    Location = "Port Industrial, Eastbank",
                    District = "Eastbank",
                    Time = "32 minutes ago",
                    Status = "Completed",
                    Timestamp = now.AddMinutes(-32),
                    ReferenceId = "DEP-104"
                },
                new()
                {
                    Id = "ACT-04",
                    Title = "Medical airlift dispatched",
                    Location = "West River Sector",
                    District = "West District",
                    Time = "45 minutes ago",
                    Status = "In Progress",
                    Timestamp = now.AddMinutes(-45),
                    ReferenceId = "SAR-088"
                },
                new()
                {
                    Id = "ACT-05",
                    Title = "Water purification unit deployed",
                    Location = "Community Hall, West District",
                    District = "West District",
                    Time = "1 hour ago",
                    Status = "Completed",
                    Timestamp = now.AddHours(-1),
                    ReferenceId = "SHEL-049"
                }
            },
            UpcomingBriefings = new List<UpcomingBriefing>
            {
                new()
                {
                    Id = "BRF-01",
                    Title = "Emergency coordination briefing",
                    Time = "Today · 14:30",
                    ScheduledAt = now.AddMinutes(42),
                    LeadOrganisation = "National Disaster Authority",
                    Location = "Operations Command Room & Virtual Bridge",
                    AttendeesCount = 24
                },
                new()
                {
                    Id = "BRF-02",
                    Title = "Inter-Agency Logistics Sync",
                    Time = "Today · 17:00",
                    ScheduledAt = now.AddHours(3),
                    LeadOrganisation = "Red Cross & UN OCHA Liaison",
                    Location = "Sector Hub East",
                    AttendeesCount = 14
                }
            },
            ActiveOperationalPointsCount = 342,
            LastSyncedAt = now
        };
    }

    private static OperationDashboardState CloneState(OperationDashboardState src)
    {
        return new OperationDashboardState
        {
            Crisis = new ActiveCrisisBriefing
            {
                Id = src.Crisis.Id,
                Title = src.Crisis.Title,
                Severity = src.Crisis.Severity,
                Level = src.Crisis.Level,
                ActiveSince = src.Crisis.ActiveSince,
                AffectedDistrictsCount = src.Crisis.AffectedDistrictsCount,
                LeadAgency = src.Crisis.LeadAgency,
                NextBriefing = src.Crisis.NextBriefing,
                NextBriefingTime = src.Crisis.NextBriefingTime
            },
            Kpis = src.Kpis.Select(k => new DashboardKpi
            {
                Title = k.Title,
                Value = k.Value,
                Detail = k.Detail,
                Icon = k.Icon,
                Color = k.Color,
                Category = k.Category,
                NumericValue = k.NumericValue
            }).ToList(),
            WeeklyTrends = src.WeeklyTrends.Select(w => new WeeklyRequestTrend
            {
                Day = w.Day,
                Received = w.Received,
                Fulfilled = w.Fulfilled,
                Date = w.Date
            }).ToList(),
            RecentActivities = src.RecentActivities.Select(a => new RecentOperationActivity
            {
                Id = a.Id,
                Title = a.Title,
                Location = a.Location,
                District = a.District,
                Time = a.Time,
                Status = a.Status,
                Timestamp = a.Timestamp,
                ReferenceId = a.ReferenceId
            }).ToList(),
            UpcomingBriefings = src.UpcomingBriefings.Select(b => new UpcomingBriefing
            {
                Id = b.Id,
                Title = b.Title,
                Time = b.Time,
                ScheduledAt = b.ScheduledAt,
                LeadOrganisation = b.LeadOrganisation,
                Location = b.Location,
                AttendeesCount = b.AttendeesCount
            }).ToList(),
            ActiveOperationalPointsCount = src.ActiveOperationalPointsCount,
            LastSyncedAt = src.LastSyncedAt
        };
    }
}
