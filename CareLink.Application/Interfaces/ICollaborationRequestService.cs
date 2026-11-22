using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface ICollaborationRequestService
{
    Task<IReadOnlyList<CollaborationRequestDto>> GetAllAsync(
        CollaborationQueryParameters? query = null,
        CancellationToken cancellationToken = default);

    Task<CollaborationRequestDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<CollaborationRequestDto> CreateAsync(CreateCollaborationRequestDto dto, CancellationToken cancellationToken = default);

    Task<CollaborationRequestDto?> UpdateAsync(string id, UpdateCollaborationRequestDto dto, CancellationToken cancellationToken = default);

    Task<CollaborationRequestDto?> UpdateStatusAsync(string id, UpdateCollaborationStatusDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<CollaborationRequestMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
