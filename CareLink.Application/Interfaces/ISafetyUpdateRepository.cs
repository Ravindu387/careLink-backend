namespace CareLink.Application.Interfaces;

using CareLink.Core.Entities;

public interface ISafetyUpdateRepository
{
    Task<IEnumerable<SafetyUpdate>> GetUpdatesAsync(string? area = null, string? level = null, string? search = null);
    Task<SafetyUpdate?> GetByIdAsync(string id);
    Task<SafetyUpdate> CreateAsync(SafetyUpdate update);
    Task<SafetyUpdate?> UpdateAsync(string id, SafetyUpdate update);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<string>> GetMonitoredAreasAsync();
}
