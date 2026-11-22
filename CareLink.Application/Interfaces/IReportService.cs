using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.Reports;

namespace CareLink.Application.Interfaces;

public interface IReportService
{
    // Administrative & Governance Reports (HEAD)
    Task<ApiResponse<ReportOverviewDto>> GetReportOverviewAsync(ReportFilterDto filter);
    Task<ApiResponse<ReportFilterOptionsDto>> GetFilterOptionsAsync();
    Task<ApiResponse<IReadOnlyList<ReportCatalogItemDto>>> GetCatalogAsync();
    Task<ApiResponse<ReportDetailDto>> GenerateReportAsync(string reportId, ReportFilterDto filter, string actor);
    Task<(byte[] Bytes, string ContentType, string FileName)> ExportReportAsync(string reportId, ExportReportRequestDto request, string actor);

    // Resource Manager Operational Reports & Analytics (Oshadi)
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
