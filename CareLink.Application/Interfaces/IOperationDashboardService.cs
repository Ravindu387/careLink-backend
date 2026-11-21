using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IOperationDashboardService
{
    Task<OperationDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DashboardKpiDto>> GetKpisAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WeeklyRequestTrendDto>> GetWeeklyTrendsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecentActivityDto>> GetRecentActivitiesAsync(int limit = 10, CancellationToken cancellationToken = default);

    Task<RecentActivityDto> AddActivityAsync(CreateActivityRequestDto dto, CancellationToken cancellationToken = default);

    Task<ActiveCrisisBriefingDto> GetActiveCrisisAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UpcomingBriefingDto>> GetUpcomingBriefingsAsync(CancellationToken cancellationToken = default);

    Task<RecentActivityDto> LogQuickIncidentAsync(LogIncidentRequestDto dto, CancellationToken cancellationToken = default);

    Task ResetDefaultDataAsync(CancellationToken cancellationToken = default);
}
