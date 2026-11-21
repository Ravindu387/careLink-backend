namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IReportService
{
    Task<ApiResponse<List<ReportItemDto>>> GetReportsAsync(ReportFilterParams? filters = null);
    Task<ApiResponse<ReportItemDto>> GetReportByIdAsync(int id);
    Task<ApiResponse<ReportItemDto>> GenerateReportAsync(GenerateReportRequest request);
    Task<ApiResponse<bool>> DeleteReportAsync(int id);

    Task<ApiResponse<List<ReportScheduleDto>>> GetSchedulesAsync();
    Task<ApiResponse<ReportScheduleDto>> CreateScheduleAsync(CreateReportScheduleRequest request);
    Task<ApiResponse<ReportScheduleDto>> ToggleScheduleStatusAsync(int id);
    Task<ApiResponse<bool>> DeleteScheduleAsync(int id);

    Task<ApiResponse<AnalyticsDashboardDto>> GetAnalyticsDashboardAsync(AnalyticsQueryParams? query = null);
    Task<(byte[] Bytes, string ContentType, string FileName)> ExportReportFileAsync(int? id, string? format = "csv", ReportFilterParams? filters = null);
}
