using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IVolunteerDashboardService
{
    Task<VolunteerDashboardOverviewDto> GetOverviewAsync();
    Task<List<DashboardStatItemDto>> GetStatsAsync();
    Task<List<DashboardUrgentTaskDto>> GetUrgentTasksAsync();
    Task<List<DashboardActivityItemDto>> GetActivitiesAsync();
    Task<DashboardActivityItemDto> AddActivityAsync(CreateDashboardActivityRequest request);
    Task<byte[]> ExportDashboardSummaryAsync();
}
