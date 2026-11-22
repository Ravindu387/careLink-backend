namespace CareLink.Application.Interfaces;

using CareLink.Application.DTOs;
using CareLink.Core.Entities;

public interface IReportRepository
{
    Task<IEnumerable<ReportItem>> GetAllReportsAsync(ReportFilterParams? filters = null);
    Task<ReportItem?> GetReportByIdAsync(int id);
    Task<ReportItem> AddReportAsync(ReportItem report);
    Task<bool> DeleteReportAsync(int id);

    Task<IEnumerable<ReportSchedule>> GetAllSchedulesAsync();
    Task<ReportSchedule?> GetScheduleByIdAsync(int id);
    Task<ReportSchedule> AddScheduleAsync(ReportSchedule schedule);
    Task<ReportSchedule?> ToggleScheduleStatusAsync(int id);
    Task<bool> DeleteScheduleAsync(int id);

    Task<AnalyticsDashboardDto> GetAnalyticsDashboardAsync(AnalyticsQueryParams? query = null);
}
