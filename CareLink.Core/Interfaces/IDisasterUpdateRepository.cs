using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IDisasterUpdateRepository
{
    Task<IEnumerable<DisasterUpdate>> GetAllAsync(DisasterUpdateQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<DisasterUpdate?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<DisasterUpdate> AddAsync(DisasterUpdate update, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(DisasterUpdate update, CancellationToken cancellationToken = default);
    Task<bool> ToggleArchiveAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmergencyContact>> GetEmergencyContactsAsync(CancellationToken cancellationToken = default);
}
