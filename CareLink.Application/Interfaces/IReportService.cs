using CareLink.Application.Common;
using CareLink.Application.DTOs.Reports;

namespace CareLink.Application.Interfaces;

public interface IReportService
{
    Task<ApiResponse<ReportOverviewDto>> GetReportOverviewAsync(ReportFilterDto filter);
    Task<ApiResponse<ReportFilterOptionsDto>> GetFilterOptionsAsync();
    Task<ApiResponse<IReadOnlyList<ReportCatalogItemDto>>> GetCatalogAsync();
    Task<ApiResponse<ReportDetailDto>> GenerateReportAsync(string reportId, ReportFilterDto filter, string actor);
    Task<(byte[] Bytes, string ContentType, string FileName)> ExportReportAsync(string reportId, ExportReportRequestDto request, string actor);
}
