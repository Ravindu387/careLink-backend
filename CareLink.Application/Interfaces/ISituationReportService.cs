using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface ISituationReportService
{
    Task<IReadOnlyList<SituationReportDto>> GetAllAsync(
        SituationReportQueryParameters? query = null,
        CancellationToken cancellationToken = default);

    Task<SituationReportDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<SituationReportDto> CreateReportAsync(
        CreateSituationReportDto dto,
        CancellationToken cancellationToken = default);

    Task<SituationReportDto?> UpdateStatusAsync(
        string id,
        UpdateReportStatusDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<SituationReportMetricsDto> GetMetricsAsync(
        CancellationToken cancellationToken = default);

    Task ResetDefaultDataAsync(
        CancellationToken cancellationToken = default);
}
