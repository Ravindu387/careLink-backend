namespace CareLink.Infrastructure.Repositories;

using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemorySettingsRepository : ISettingsRepository
{
    private ResourceManagerSettings _settings = new();
    private readonly Lock _lock = new();

    public Task<ResourceManagerSettings> GetSettingsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(Clone(_settings));
        }
    }

    public Task<ResourceManagerSettings> UpdateSettingsAsync(ResourceManagerSettings settings)
    {
        lock (_lock)
        {
            settings.UpdatedAt = DateTime.UtcNow;
            _settings = Clone(settings);
            return Task.FromResult(Clone(_settings));
        }
    }

    public Task<ResourceManagerSettings> ResetSettingsAsync()
    {
        lock (_lock)
        {
            _settings = new ResourceManagerSettings
            {
                UpdatedAt = DateTime.UtcNow
            };
            return Task.FromResult(Clone(_settings));
        }
    }

    private static ResourceManagerSettings Clone(ResourceManagerSettings source)
    {
        return new ResourceManagerSettings
        {
            Profile = new ProfileSettings
            {
                FullName = source.Profile.FullName,
                Email = source.Profile.Email,
                Phone = source.Profile.Phone,
                Warehouse = source.Profile.Warehouse
            },
            Inventory = new InventoryRulesSettings
            {
                LowStock = source.Inventory.LowStock,
                CriticalStock = source.Inventory.CriticalStock,
                ExpiryDays = source.Inventory.ExpiryDays,
                AutoReorder = source.Inventory.AutoReorder,
                TrackExpiry = source.Inventory.TrackExpiry,
                AllowNegative = source.Inventory.AllowNegative
            },
            Dispatch = new DispatchPreferencesSettings
            {
                DefaultVehicle = source.Dispatch.DefaultVehicle,
                Approval = source.Dispatch.Approval,
                Proof = source.Dispatch.Proof,
                Tracking = source.Dispatch.Tracking,
                DelayAlert = source.Dispatch.DelayAlert
            },
            Notifications = new NotificationPreferencesSettings
            {
                Email = source.Notifications.Email,
                System = source.Notifications.System,
                Critical = source.Notifications.Critical,
                LowStock = source.Notifications.LowStock,
                Dispatch = source.Notifications.Dispatch,
                Weekly = source.Notifications.Weekly
            },
            Security = new SecuritySettings
            {
                TwoFactor = source.Security.TwoFactor,
                SessionTimeout = source.Security.SessionTimeout,
                LoginAlerts = source.Security.LoginAlerts
            },
            UpdatedAt = source.UpdatedAt
        };
    }
}
