using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IShelterReportsService
{
    Task<ShelterReportsOverviewDto> GetOverviewAsync(ShelterReportsFilterDto filter);
    Task<ShelterMetricsDto> GetMetricsAsync(ShelterReportsFilterDto filter);
    Task<List<ShelterTrendDataPoint>> GetTrendDataAsync(ShelterReportsFilterDto filter);
    Task<List<ShelterReportItem>> GetReportLibraryAsync();
    Task<ShelterReportItem?> GetReportByIdAsync(int id);
    Task<byte[]> ExportOverviewCsvAsync(ShelterReportsFilterDto filter);
    Task<byte[]> ExportReportItemCsvAsync(string reportTitle, ShelterReportsFilterDto filter);
}
