using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IShelterRepository
{
    Task<IEnumerable<Shelter>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Shelter?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Shelter> AddAsync(Shelter shelter, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Shelter> Items, int TotalCount)> GetPagedAsync(ShelterQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<(int Registered, int Operational, int NearCapacity, int Maintenance)> GetSummaryMetricsAsync(CancellationToken cancellationToken = default);
}
