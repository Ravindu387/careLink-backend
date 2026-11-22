using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IAuditTimelineService
{
    Task<IReadOnlyList<AuditEventResponseDto>> GetAllAsync(
        AuditQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<AuditEventResponseDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<AuditEventResponseDto> LogEventAsync(
        CreateAuditEventDto dto,
        CancellationToken cancellationToken = default);

    Task<AuditTimelineMetricsDto> GetMetricsAsync(
        CancellationToken cancellationToken = default);

    Task<bool> VerifyLogIntegrityAsync(
        CancellationToken cancellationToken = default);

    Task ResetDataAsync(
        CancellationToken cancellationToken = default);
}
