using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public interface IOperationDashboardRepository
{
    Task<OperationDashboardState> GetDashboardStateAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DashboardKpi>> GetKpisAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WeeklyRequestTrend>> GetWeeklyTrendsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecentOperationActivity>> GetRecentActivitiesAsync(int limit = 10, CancellationToken cancellationToken = default);

    Task<RecentOperationActivity> AddActivityAsync(RecentOperationActivity activity, CancellationToken cancellationToken = default);

    Task<ActiveCrisisBriefing> GetActiveCrisisAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UpcomingBriefing>> GetUpcomingBriefingsAsync(CancellationToken cancellationToken = default);

    Task<RecentOperationActivity> LogQuickIncidentAsync(
        string title,
        string district,
        string location,
        string severity,
        CancellationToken cancellationToken = default);

    Task ResetDefaultDataAsync(CancellationToken cancellationToken = default);
}
