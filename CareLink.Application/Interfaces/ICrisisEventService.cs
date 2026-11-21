using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface ICrisisEventService
{
    Task<IReadOnlyList<CrisisEventResponseDto>> GetAllAsync(
        CrisisEventQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<CrisisEventResponseDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<CrisisEventResponseDto> CreateAsync(
        CreateCrisisEventDto dto,
        CancellationToken cancellationToken = default);

    Task<CrisisEventResponseDto?> UpdateAsync(
        string id,
        UpdateCrisisEventDto dto,
        CancellationToken cancellationToken = default);

    Task<CrisisEventResponseDto?> UpdateStatusAsync(
        string id,
        UpdateCrisisStatusDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<CrisisEventMetricsDto> GetMetricsAsync(
        CancellationToken cancellationToken = default);

    Task ResetDataAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SharedCrisisEventDto>> GetSharedEventsAsync(
        SharedCrisisQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<SharedCrisisMetricsDto> GetSharedMetricsAsync(
        CancellationToken cancellationToken = default);

    Task<CrisisAccessRequestResponseDto> CreateAccessRequestAsync(
        CreateCrisisAccessRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CrisisAccessRequestResponseDto>> GetAccessRequestsAsync(
        CancellationToken cancellationToken = default);
}
