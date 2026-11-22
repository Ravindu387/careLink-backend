using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IFieldActivityService
{
    Task<IReadOnlyList<FieldActivityDto>> GetAllAsync(
        FieldActivityQueryParameters? query = null,
        CancellationToken cancellationToken = default);

    Task<FieldActivityDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<FieldActivityDto> CreateAsync(CreateFieldActivityDto dto, CancellationToken cancellationToken = default);

    Task<FieldActivityDto?> UpdateAsync(string id, UpdateFieldActivityDto dto, CancellationToken cancellationToken = default);

    Task<FieldActivityDto?> UpdateStatusAsync(string id, UpdateFieldActivityStatusDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<FieldActivityMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
