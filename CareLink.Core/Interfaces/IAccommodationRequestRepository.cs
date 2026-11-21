using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IAccommodationRequestRepository
{
    Task<AccommodationRequest?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccommodationRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<AccommodationRequest> Items, int TotalCount)> GetPagedAsync(AccommodationRequestQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default);
    Task<AccommodationRequest> AddAsync(AccommodationRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(string id, string newStatus, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
