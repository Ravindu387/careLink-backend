using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IReportsService
{
    Task<ReportsAnalyticsDto> GetAnalyticsAsync(
        ReportsAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ReportDefinitionDto>> GetReportDefinitionsAsync(
        CancellationToken cancellationToken = default);

    Task<string> ExportReportCsvAsync(
        string reportTitle,
        ReportsAnalyticsQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
