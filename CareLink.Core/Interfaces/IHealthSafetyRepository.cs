using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IHealthSafetyRepository
{
    Task<IEnumerable<HealthSafetyIncident>> GetAllAsync(HealthSafetyIncidentQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<HealthSafetyIncident?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<HealthSafetyIncident> AddAsync(HealthSafetyIncident incident, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(HealthSafetyIncident incident, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SafetyCheck>> GetSafetyChecksAsync(CancellationToken cancellationToken = default);
}
