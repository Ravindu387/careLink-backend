using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface ILiveOperationsMapService
{
    Task<IReadOnlyList<OperationPointDto>> GetAllAsync(
        OperationPointQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<OperationPointDto?> GetByIdAsync(
        string idOrRequestId,
        CancellationToken cancellationToken = default);

    Task<OperationPointDto> CreateAsync(
        CreateOperationPointDto dto,
        CancellationToken cancellationToken = default);

    Task<OperationPointDto?> UpdateAsync(
        string id,
        UpdateOperationPointDto dto,
        CancellationToken cancellationToken = default);

    Task<OperationPointDto?> AssignTeamAsync(
        string id,
        AssignOperationPointDto dto,
        CancellationToken cancellationToken = default);

    Task<OperationPointDto?> UpdateCoordinatesAsync(
        string id,
        UpdateCoordinatesDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<LiveOperationsMapMetricsDto> GetMetricsAsync(
        CancellationToken cancellationToken = default);

    Task ResetDefaultDataAsync(
        CancellationToken cancellationToken = default);
}
