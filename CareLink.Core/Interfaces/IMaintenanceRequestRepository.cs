using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IMaintenanceRequestRepository
{
    Task<(IEnumerable<MaintenanceRequest> Items, int TotalCount)> GetPagedAsync(MaintenanceRequestQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MaintenanceRequest?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<MaintenanceRequest> AddAsync(MaintenanceRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(MaintenanceRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default);
}
