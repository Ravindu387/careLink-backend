using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemoryPartnerReportRepository : IPartnerReportRepository
{
    private readonly ConcurrentDictionary<string, PartnerReport> _reports = new();
    private readonly object _lock = new();

    public InMemoryPartnerReportRepository()
    {
        SeedDefaultData();
    }

    public Task<IReadOnlyList<PartnerReport>> GetAllAsync(
        PartnerReportFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _reports.Values.AsEnumerable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(r =>
                    r.Id.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.Title.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.Type.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.Owner.Contains(s, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Tab) &&
                !filter.Tab.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var tab = filter.Tab.Trim();
                if (tab.Equals("Needs action", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Status.Equals("Draft", StringComparison.OrdinalIgnoreCase) ||
                                             r.Status.Equals("Under Review", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Status.Equals("Draft", StringComparison.OrdinalIgnoreCase) ||
                                             r.Status.Equals("Under Review", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Status.Equals("Submitted", StringComparison.OrdinalIgnoreCase) ||
                                             r.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase));
                }
                else if (tab.Equals("Archived", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(r => r.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase));
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Type) &&
                !filter.Type.Equals("All report types", StringComparison.OrdinalIgnoreCase))
            {
                var t = filter.Type.Trim();
                query = query.Where(r => r.Type.Equals(t, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                !filter.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
            {
                var st = filter.Status.Trim();
                query = query.Where(r => r.Status.Equals(st, StringComparison.OrdinalIgnoreCase));
            }
        }

        var list = query
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<PartnerReport>>(list);
    }

    public Task<PartnerReport?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _reports.TryGetValue(id.Trim(), out var report);
        return Task.FromResult(report);
    }

    public Task<PartnerReport> AddAsync(PartnerReport report, CancellationToken cancellationToken = default)
    {
        _reports[report.Id] = report;
        return Task.FromResult(report);
    }

    public Task<PartnerReport?> UpdateAsync(PartnerReport report, CancellationToken cancellationToken = default)
    {
        if (!_reports.ContainsKey(report.Id)) return Task.FromResult<PartnerReport?>(null);
        _reports[report.Id] = report;
        return Task.FromResult<PartnerReport?>(report);
    }

    public Task<bool> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default)
    {
        if (!_reports.TryGetValue(id.Trim(), out var existing)) return Task.FromResult(false);

        existing.Status = status.Trim();
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        if (status.Equals("Submitted", StringComparison.OrdinalIgnoreCase))
        {
            existing.Submitted = DateTimeOffset.UtcNow.ToString("dd MMM - HH:mm");
            existing.Progress = 100;
        }
        else if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            existing.Progress = 100;
        }

        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_reports.TryRemove(id.Trim(), out _));
    }

    public Task<PartnerReportsAnalytics> GetAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var all = _reports.Values.ToList();
        var analytics = new PartnerReportsAnalytics
        {
            ReportsSubmitted = "28",
            OnTimeRate = "96%",
            OnTimeTrend = "↑ 4% this month",
            PeopleReached = "18.6K",
            PeopleReachedSubtitle = "Across 4 sectors",
            NextDeadline = "4h 12m",
            NextDeadlineSubtitle = "Daily activity report",
            Reminder = new DailyReportReminder
            {
                Title = "Daily Partner Activity Report · Monsoon Floods 2025",
                Deadline = "DUE TODAY - 18:00",
                Subtitle = "Progress saved · 82% complete · 3 sections require review",
                Progress = 82
            },
            WeeklyReach = new List<WeeklyReachDataPoint>
            {
                new() { Day = "Mon", Planned = 3200, Reached = 2800 },
                new() { Day = "Tue", Planned = 5400, Reached = 4200 },
                new() { Day = "Wed", Planned = 7600, Reached = 6800 },
                new() { Day = "Thu", Planned = 9200, Reached = 8200 }
            },
            PerformanceTargets = new List<PerformanceTargetItem>
            {
                new() { Title = "Referral acceptance", Target = "Target 90%", Value = 92 },
                new() { Title = "Resource delivery", Target = "Target 95%", Value = 88 },
                new() { Title = "Shelter coverage", Target = "Target 85%", Value = 96 },
                new() { Title = "Field activity completion", Target = "Target 90%", Value = 82 }
            },
            Schedules = new List<ReportingScheduleItem>
            {
                new() { Name = "Daily Partner Activity Report", Deadline = "Every day · 18:00", Status = "Due today" },
                new() { Name = "Weekly Operational Report", Deadline = "Every Friday · 16:00", Status = "Upcoming" },
                new() { Name = "Monthly Performance Report", Deadline = "Last day of month", Status = "Upcoming" }
            },
            TotalReports = all.Count,
            NeedsActionCount = all.Count(r => r.Status.Equals("Draft", StringComparison.OrdinalIgnoreCase) ||
                                              r.Status.Equals("Under Review", StringComparison.OrdinalIgnoreCase)),
            ActiveCount = all.Count(r => r.Status.Equals("Draft", StringComparison.OrdinalIgnoreCase) ||
                                         r.Status.Equals("Under Review", StringComparison.OrdinalIgnoreCase)),
            CompletedCount = all.Count(r => r.Status.Equals("Submitted", StringComparison.OrdinalIgnoreCase) ||
                                            r.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase)),
            ArchivedCount = all.Count(r => r.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase)),
            LastUpdated = DateTimeOffset.UtcNow
        };

        return Task.FromResult(analytics);
    }

    public Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _reports.Clear();
            SeedDefaultData();
            return Task.CompletedTask;
        }
    }

    private void SeedDefaultData()
    {
        var now = DateTimeOffset.UtcNow;
        var initial = new List<PartnerReport>
        {
            new()
            {
                Id = "RPT-1042",
                Title = "Daily Partner Activity Report",
                Type = "Daily Report",
                Owner = "Sara Khan",
                Period = "22 Jun 2025",
                Submitted = "22 Jun - 12:18",
                Status = "Submitted",
                Progress = 100,
                Description = "Overview of field distributions, staff shifts, and partner coordination meetings for today.",
                OrganizationId = "ORG-027",
                CreatedAt = now.AddHours(-4),
                UpdatedAt = now.AddHours(-4)
            },
            new()
            {
                Id = "RPT-1038",
                Title = "Food Distribution Progress Report",
                Type = "Operational Report",
                Owner = "Michael Torres",
                Period = "21 Jun 2025",
                Submitted = "21 Jun - 17:30",
                Status = "Approved",
                Progress = 100,
                Description = "Verification of dry food rations delivered to 420 households in North Ward Zone 3.",
                OrganizationId = "ORG-027",
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now.AddDays(-1)
            },
            new()
            {
                Id = "RPT-1034",
                Title = "WASH Activity Report",
                Type = "Sector Report",
                Owner = "Nadia Silva",
                Period = "20 Jun 2025",
                Submitted = "20 Jun - 15:10",
                Status = "Under Review",
                Progress = 82,
                Description = "Water testing, chlorination adjustments, and sanitation station maintenance across Eastbank camps.",
                OrganizationId = "ORG-027",
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddDays(-2)
            },
            new()
            {
                Id = "RPT-1029",
                Title = "Shelter Assessment Report",
                Type = "Assessment Report",
                Owner = "Sara Khan",
                Period = "19 Jun 2025",
                Submitted = "19 Jun - 14:00",
                Status = "Draft",
                Progress = 65,
                Description = "Structural safety appraisal of evacuation shelters and tarpaulin requirements.",
                OrganizationId = "ORG-027",
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddDays(-3)
            },
            new()
            {
                Id = "RPT-1020",
                Title = "Protection Outreach Summary",
                Type = "Operational Report",
                Owner = "Nadia Silva",
                Period = "18 Jun 2025",
                Submitted = "18 Jun - 11:30",
                Status = "Approved",
                Progress = 100,
                Description = "Psychosocial first aid counseling and child-friendly space monitoring summary.",
                OrganizationId = "ORG-027",
                CreatedAt = now.AddDays(-4),
                UpdatedAt = now.AddDays(-4)
            },
            new()
            {
                Id = "RPT-1015",
                Title = "Emergency Health Clinic Log",
                Type = "Sector Report",
                Owner = "Sara Khan",
                Period = "17 Jun 2025",
                Submitted = "17 Jun - 16:45",
                Status = "Archived",
                Progress = 100,
                Description = "Outreach mobile clinic triage records and medical supply replenishment logs.",
                OrganizationId = "ORG-027",
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-5)
            }
        };

        foreach (var r in initial)
        {
            _reports[r.Id] = r;
        }
    }
}
