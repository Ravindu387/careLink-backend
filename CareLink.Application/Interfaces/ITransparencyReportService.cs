using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface ITransparencyReportService
{
    Task<PagedResultDto<TransparencyReportDto>> GetPagedAsync(
        TransparencyReportQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TransparencyReportDto>> GetAllAsync(
        TransparencyReportQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<TransparencyReportDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<TransparencyReportDto> CreateAsync(CreateTransparencyReportDto dto, CancellationToken cancellationToken = default);

    Task<TransparencyReportDto?> UpdateAsync(string id, UpdateTransparencyReportDto dto, CancellationToken cancellationToken = default);

    Task<bool> PublishAsync(string id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<TransparencyMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);
}
