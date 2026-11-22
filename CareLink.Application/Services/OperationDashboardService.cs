using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class OperationDashboardService : IOperationDashboardService
{
    private readonly IOperationDashboardRepository _repository;

    public OperationDashboardService(IOperationDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<OperationDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var state = await _repository.GetDashboardStateAsync(cancellationToken);

        return new OperationDashboardDto
        {
            Crisis = MapCrisis(state.Crisis),
            Kpis = state.Kpis.Select(MapKpi).ToList(),
            WeeklyTrends = state.WeeklyTrends.Select(MapTrend).ToList(),
            RecentActivities = state.RecentActivities.Select(MapActivity).ToList(),
            UpcomingBriefings = state.UpcomingBriefings.Select(MapBriefing).ToList(),
            ActiveOperationalPointsCount = state.ActiveOperationalPointsCount,
            LastSyncedAt = state.LastSyncedAt
        };
    }

    public async Task<IReadOnlyList<DashboardKpiDto>> GetKpisAsync(CancellationToken cancellationToken = default)
    {
        var kpis = await _repository.GetKpisAsync(cancellationToken);
        return kpis.Select(MapKpi).ToList();
    }

    public async Task<IReadOnlyList<WeeklyRequestTrendDto>> GetWeeklyTrendsAsync(CancellationToken cancellationToken = default)
    {
        var trends = await _repository.GetWeeklyTrendsAsync(cancellationToken);
        return trends.Select(MapTrend).ToList();
    }

    public async Task<IReadOnlyList<RecentActivityDto>> GetRecentActivitiesAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        var activities = await _repository.GetRecentActivitiesAsync(limit, cancellationToken);
        return activities.Select(MapActivity).ToList();
    }

    public async Task<RecentActivityDto> AddActivityAsync(CreateActivityRequestDto dto, CancellationToken cancellationToken = default)
    {
        var activity = new RecentOperationActivity
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            Title = dto.Title.Trim(),
            Location = dto.Location.Trim(),
            District = dto.District ?? "Greater Riverton",
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "New" : dto.Status,
            ReferenceId = dto.ReferenceId,
            Timestamp = DateTimeOffset.UtcNow,
            Time = "Just now"
        };

        var created = await _repository.AddActivityAsync(activity, cancellationToken);
        return MapActivity(created);
    }

    public async Task<ActiveCrisisBriefingDto> GetActiveCrisisAsync(CancellationToken cancellationToken = default)
    {
        var crisis = await _repository.GetActiveCrisisAsync(cancellationToken);
        return MapCrisis(crisis);
    }

    public async Task<IReadOnlyList<UpcomingBriefingDto>> GetUpcomingBriefingsAsync(CancellationToken cancellationToken = default)
    {
        var briefings = await _repository.GetUpcomingBriefingsAsync(cancellationToken);
        return briefings.Select(MapBriefing).ToList();
    }

    public async Task<RecentActivityDto> LogQuickIncidentAsync(LogIncidentRequestDto dto, CancellationToken cancellationToken = default)
    {
        var activity = await _repository.LogQuickIncidentAsync(
            dto.Title.Trim(),
            dto.District.Trim(),
            dto.Location.Trim(),
            dto.Severity,
            cancellationToken);

        return MapActivity(activity);
    }

    public async Task ResetDefaultDataAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetDefaultDataAsync(cancellationToken);
    }

    private static DashboardKpiDto MapKpi(DashboardKpi kpi) => new()
    {
        Title = kpi.Title,
        Value = kpi.Value,
        Detail = kpi.Detail,
        Icon = kpi.Icon,
        Color = kpi.Color,
        Category = kpi.Category,
        NumericValue = kpi.NumericValue
    };

    private static ActiveCrisisBriefingDto MapCrisis(ActiveCrisisBriefing c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        Severity = c.Severity,
        Level = c.Level,
        ActiveSince = c.ActiveSince,
        AffectedDistrictsCount = c.AffectedDistrictsCount,
        LeadAgency = c.LeadAgency,
        NextBriefing = c.NextBriefing,
        NextBriefingTime = c.NextBriefingTime
    };

    private static WeeklyRequestTrendDto MapTrend(WeeklyRequestTrend t) => new()
    {
        Day = t.Day,
        Received = t.Received,
        Fulfilled = t.Fulfilled,
        Date = t.Date
    };

    private static RecentActivityDto MapActivity(RecentOperationActivity a)
    {
        var timeStr = a.Time;
        if (string.IsNullOrWhiteSpace(timeStr))
        {
            var elapsed = DateTimeOffset.UtcNow - a.Timestamp;
            if (elapsed.TotalMinutes < 2) timeStr = "Just now";
            else if (elapsed.TotalMinutes < 60) timeStr = $"{(int)elapsed.TotalMinutes} minutes ago";
            else if (elapsed.TotalHours < 24) timeStr = $"{(int)elapsed.TotalHours} hours ago";
            else timeStr = a.Timestamp.ToString("MMM dd, HH:mm");
        }

        return new RecentActivityDto
        {
            Id = a.Id,
            Title = a.Title,
            Location = a.Location,
            District = a.District,
            Time = timeStr,
            Status = a.Status,
            Timestamp = a.Timestamp,
            ReferenceId = a.ReferenceId
        };
    }

    private static UpcomingBriefingDto MapBriefing(UpcomingBriefing b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        Time = b.Time,
        ScheduledAt = b.ScheduledAt,
        LeadOrganisation = b.LeadOrganisation,
        Location = b.Location,
        AttendeesCount = b.AttendeesCount
    };
}
