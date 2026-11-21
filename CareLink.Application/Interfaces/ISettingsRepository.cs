namespace CareLink.Application.Interfaces;

using CareLink.Core.Entities;

public interface ISettingsRepository
{
    Task<ResourceManagerSettings> GetSettingsAsync();
    Task<ResourceManagerSettings> UpdateSettingsAsync(ResourceManagerSettings settings);
    Task<ResourceManagerSettings> ResetSettingsAsync();
}
