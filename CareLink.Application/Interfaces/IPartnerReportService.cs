using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IPartnerReportService
{
    Task<IReadOnlyList<PartnerReportDto>> GetAllAsync(
        PartnerReportQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<PartnerReportDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<PartnerReportDto> CreateAsync(
        CreatePartnerReportDto dto,
        CancellationToken cancellationToken = default);

    Task<PartnerReportDto?> UpdateAsync(
        string id,
        UpdatePartnerReportDto dto,
        CancellationToken cancellationToken = default);

    Task<PartnerReportDto?> UpdateStatusAsync(
        string id,
        string status,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<PartnerReportsAnalyticsDto> GetAnalyticsAsync(
        CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(
        CancellationToken cancellationToken = default);
}
