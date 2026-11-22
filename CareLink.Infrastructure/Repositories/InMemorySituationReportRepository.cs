using System.Collections.Concurrent;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Infrastructure.Repositories;

public class InMemorySituationReportRepository : ISituationReportRepository
{
    private readonly ConcurrentDictionary<string, SituationReport> _reports = new(StringComparer.OrdinalIgnoreCase);

    public InMemorySituationReportRepository()
    {
        SeedDefaultData();
    }

    public Task<IReadOnlyList<SituationReport>> GetAllAsync(
        SituationReportFilterParams? filter = null,
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
                    r.Event.Contains(s, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Event) &&
                !filter.Event.Equals("All crisis events", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Event.Equals(filter.Event.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                !filter.Status.Equals("All report states", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Status.Equals(filter.Status.Trim(), StringComparison.OrdinalIgnoreCase));
            }
        }

        var list = query
            .OrderByDescending(r => r.Id)
            .ToList();

        return Task.FromResult<IReadOnlyList<SituationReport>>(list);
    }

    public Task<SituationReport?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Task.FromResult<SituationReport?>(null);

        _reports.TryGetValue(id.Trim(), out var report);
        return Task.FromResult(report);
    }

    public Task<SituationReport> AddAsync(SituationReport report, CancellationToken cancellationToken = default)
    {
        _reports[report.Id] = report;
        return Task.FromResult(report);
    }

    public Task<SituationReport?> UpdateStatusAsync(
        string id,
        string status,
        string approvedBy,
        int recipients,
        CancellationToken cancellationToken = default)
    {
        if (!_reports.TryGetValue(id.Trim(), out var existing))
        {
            return Task.FromResult<SituationReport?>(null);
        }

        existing.Status = status;
        existing.ApprovedBy = approvedBy;
        existing.Recipients = recipients;

        if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            existing.ApprovedAt = DateTimeOffset.UtcNow;
        }
        else if (status.Equals("Published", StringComparison.OrdinalIgnoreCase))
        {
            existing.PublishedAt = DateTimeOffset.UtcNow;
            if (!existing.ApprovedAt.HasValue) existing.ApprovedAt = DateTimeOffset.UtcNow;
        }

        _reports[existing.Id] = existing;
        return Task.FromResult<SituationReport?>(existing);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var removed = _reports.TryRemove(id.Trim(), out _);
        return Task.FromResult(removed);
    }

    public Task<SituationReportMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var all = _reports.Values.ToList();

        var publishedCount = all.Count(r =>
            r.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) ||
            r.Status.Equals("Published", StringComparison.OrdinalIgnoreCase));

        var draftCount = all.Count(r =>
            r.Status.Equals("Draft", StringComparison.OrdinalIgnoreCase));

        var totalRecipients = all.Sum(r => r.Recipients);

        var latestApproved = all
            .Where(r => r.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) ||
                        r.Status.Equals("Published", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.Id)
            .FirstOrDefault() ?? all.FirstOrDefault();

        var metrics = new SituationReportMetrics
        {
            PublishedCount = publishedCount,
            DraftCount = draftCount,
            TotalRecipients = totalRecipients,
            NextReportDue = "2h 18m",
            NextReportId = $"SITREP {all.Count + 21:D3}",
            LatestApproved = latestApproved,
            TotalReports = all.Count
        };

        return Task.FromResult(metrics);
    }

    public Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        _reports.Clear();
        SeedDefaultData();
        return Task.CompletedTask;
    }

    private void SeedDefaultData()
    {
        var initial = new List<SituationReport>
        {
            new()
            {
                Id = "SITREP 024",
                Title = "Greater Riverton Flood Response",
                Period = "22 June 2025 · 08:00–12:00 UTC",
                Status = "Approved",
                PreparedBy = "Operations Analysis Team",
                ApprovedBy = "Amina Mensah",
                Recipients = 48,
                Event = "Monsoon Floods 2025",
                ExecutiveSummary = "Kelani basin water levels stabilizing at 8.2m. Search and rescue operations completed in Sector 3 with 1,284 individuals evacuated safely to designated community shelters.",
                KeyHighlights = "Primary drinking water and field sanitation lines restored across North Ward. 14 partner NGOs deployed with 28 vehicle assets active in the operational theater.",
                OperationalChallenges = "Secondary access roads along Eastbank remain submerged by 0.4m mud deposits. Heavy construction clearance teams requested.",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-3),
                ApprovedAt = DateTimeOffset.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = "SITREP 023",
                Title = "Greater Riverton Flood Response",
                Period = "22 June 2025 · 06:00–08:00 UTC",
                Status = "Published",
                PreparedBy = "Operations Analysis Team",
                ApprovedBy = "Amina Mensah",
                Recipients = 48,
                Event = "Monsoon Floods 2025",
                ExecutiveSummary = "Overnight precipitation reached 142mm. Red alert flash flood warnings issued for Greater Riverton lowland zones.",
                KeyHighlights = "Evacuation shelters at Central High and Community Sports Complex activated at 65% capacity.",
                OperationalChallenges = "Grid power outages affecting communication relay towers in Sector 4.",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-7),
                ApprovedAt = DateTimeOffset.UtcNow.AddHours(-6),
                PublishedAt = DateTimeOffset.UtcNow.AddHours(-5)
            },
            new()
            {
                Id = "SITREP 022",
                Title = "North Ridge Landslide Response",
                Period = "21 June 2025 · 16:00–20:00 UTC",
                Status = "Approved",
                PreparedBy = "Emergency Coordination Team",
                ApprovedBy = "Amina Mensah",
                Recipients = 32,
                Event = "North Ridge Landslide",
                ExecutiveSummary = "Geological surveillance team confirmed slope stabilization along arterial highway B-24.",
                KeyHighlights = "All 42 stranded passenger vehicles cleared safely with zero casualties.",
                OperationalChallenges = "Intermittent rainfall continues to risk small-scale soil creep.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                ApprovedAt = DateTimeOffset.UtcNow.AddDays(-1).AddHours(2)
            },
            new()
            {
                Id = "SITREP 021",
                Title = "Eastbank Emergency Response",
                Period = "20 June 2025 · 08:00–12:00 UTC",
                Status = "Draft",
                PreparedBy = "Operations Analysis Team",
                ApprovedBy = "Pending approval",
                Recipients = 0,
                Event = "Monsoon Floods 2025",
                ExecutiveSummary = "Initial damage assessment and resource requisition compilation for Eastbank Industrial Corridor.",
                KeyHighlights = "Drafted resource distribution schedules for 6 community relief points.",
                OperationalChallenges = "Awaiting field casualty reports from local civil protection officers.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-2)
            }
        };

        foreach (var r in initial)
        {
            _reports[r.Id] = r;
        }
    }
}
