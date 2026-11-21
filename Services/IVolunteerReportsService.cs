using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public interface IVolunteerReportsService
{
    Task<VolunteerReportsOverviewDto> GetOverviewAsync(VolunteerReportsQueryDto query);
    Task<VolunteerAnalyticsSummaryDto> GetAnalyticsAsync(VolunteerReportsQueryDto query);
    Task<List<TrendDataPoint>> GetTrendDataAsync(VolunteerReportsQueryDto query);
    Task<List<SkillHoursBreakdown>> GetSkillBreakdownAsync(VolunteerReportsQueryDto query);
    Task<List<VolunteerReportItem>> GetReportLibraryAsync();
    Task<VolunteerReportItem?> GetReportByIdAsync(int id);
    Task<byte[]> ExportOverviewCsvAsync(VolunteerReportsQueryDto query);
    Task<byte[]> ExportReportItemCsvAsync(int id, VolunteerReportsQueryDto query);
}
