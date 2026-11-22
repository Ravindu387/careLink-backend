using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface ICaseReportsService
{
    Task<ReportsAnalyticsResponse> GetReportsAnalyticsAsync(ReportsFilterDto filters);
    Task<List<ReportItem>> GetReportLibraryAsync();
    Task<byte[]> ExportReportsCsvAsync(ReportsFilterDto filters);
}
