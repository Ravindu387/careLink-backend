using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IPriorityQueueService
{
    Task<IReadOnlyList<PriorityQueueItemDto>> GetAllAsync(
        PriorityQueueQueryParameters? query = null,
        CancellationToken cancellationToken = default);

    Task<PagedPriorityQueueDto> GetPagedAsync(
        PriorityQueueQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<PriorityQueueItemDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<PriorityQueueItemDto> CreateAsync(
        CreatePriorityRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<PriorityQueueItemDto?> AssignTeamAsync(
        string id,
        AssignTeamDto dto,
        CancellationToken cancellationToken = default);

    Task<PriorityQueueItemDto?> UpdateStatusAsync(
        string id,
        UpdatePriorityStatusDto dto,
        CancellationToken cancellationToken = default);

    Task<PriorityQueueItemDto?> UpdatePriorityAsync(
        string id,
        UpdatePriorityLevelDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<PriorityQueueMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SlaPolicyDto>> GetSlaPoliciesAsync(CancellationToken cancellationToken = default);

    Task ResetDefaultDataAsync(CancellationToken cancellationToken = default);
}
