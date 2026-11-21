using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public interface ISettingsRepository
{
    Task<UserSettings?> GetAsync();
    Task<UserSettings> UpsertAsync(UserSettings settings);
}
