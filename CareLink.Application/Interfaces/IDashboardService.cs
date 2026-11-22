using CareLink.Application.Common;
using CareLink.Application.DTOs.Dashboard;

namespace CareLink.Application.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<DashboardDataDto>> GetDashboardDataAsync();
    Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync();
    Task<ApiResponse<IReadOnlyList<DashboardChartPointDto>>> GetWeeklyTrendAsync();
    Task<ApiResponse<IReadOnlyList<DashboardActivityItemDto>>> GetRecentActivitiesAsync();
    Task<ApiResponse<IReadOnlyList<DashboardAlertItemDto>>> GetActiveAlertsAsync();
}
