using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IShelterService
{
    Task<IEnumerable<ShelterDto>> GetAllSheltersAsync(CancellationToken cancellationToken = default);
    Task<ShelterDto?> GetShelterByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<PagedResultDto<ShelterDto>> GetSheltersPagedAsync(ShelterQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<ShelterSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<ShelterDto> CreateShelterAsync(CreateShelterDto dto, CancellationToken cancellationToken = default);
}
