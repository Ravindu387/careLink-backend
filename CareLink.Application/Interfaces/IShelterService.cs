using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IShelterService
{
    Task<IReadOnlyList<ShelterDto>> GetAllAsync(
        ShelterQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<ShelterDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ShelterDto> CreateAsync(CreateShelterDto dto, CancellationToken cancellationToken = default);

    Task<ShelterDto?> UpdateAsync(int id, UpdateShelterDto dto, CancellationToken cancellationToken = default);

    Task<ShelterDto?> UpdateOccupancyAsync(int id, int occupied, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<ShelterMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
