using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IAccommodationRequestService
{
    Task<PagedResultDto<AccommodationRequestDto>> GetPagedRequestsAsync(AccommodationRequestQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccommodationRequestDto>> GetAllRequestsAsync(CancellationToken cancellationToken = default);
    Task<AccommodationRequestDto?> GetRequestByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<AccommodationRequestCountsDto> GetStatusCountsAsync(CancellationToken cancellationToken = default);
    Task<AccommodationRequestDto> CreateRequestAsync(CreateAccommodationRequestDto dto, CancellationToken cancellationToken = default);
    Task<AccommodationRequestDto?> UpdateStatusAsync(string id, UpdateAccommodationRequestStatusDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteRequestAsync(string id, CancellationToken cancellationToken = default);
}
