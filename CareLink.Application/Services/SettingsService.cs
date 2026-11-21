namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _repository;

    public SettingsService(ISettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<ResourceManagerSettingsDto>> GetSettingsAsync()
    {
        var settings = await _repository.GetSettingsAsync();
        var dto = MapToDto(settings);
        return ApiResponse<ResourceManagerSettingsDto>.SuccessResponse(dto, "Settings retrieved successfully");
    }

    public async Task<ApiResponse<ResourceManagerSettingsDto>> UpdateSettingsAsync(UpdateSettingsRequest request)
    {
        var current = await _repository.GetSettingsAsync();

        if (request.Profile != null)
        {
            current.Profile.FullName = request.Profile.FullName ?? current.Profile.FullName;
            current.Profile.Email = request.Profile.Email ?? current.Profile.Email;
            current.Profile.Phone = request.Profile.Phone ?? current.Profile.Phone;
            current.Profile.Warehouse = request.Profile.Warehouse ?? current.Profile.Warehouse;
        }

        if (request.Inventory != null)
        {
            current.Inventory.LowStock = request.Inventory.LowStock;
            current.Inventory.CriticalStock = request.Inventory.CriticalStock;
            current.Inventory.ExpiryDays = request.Inventory.ExpiryDays;
            current.Inventory.AutoReorder = request.Inventory.AutoReorder;
            current.Inventory.TrackExpiry = request.Inventory.TrackExpiry;
            current.Inventory.AllowNegative = request.Inventory.AllowNegative;
        }

        if (request.Dispatch != null)
        {
            current.Dispatch.DefaultVehicle = request.Dispatch.DefaultVehicle;
            current.Dispatch.Approval = request.Dispatch.Approval;
            current.Dispatch.Proof = request.Dispatch.Proof;
            current.Dispatch.Tracking = request.Dispatch.Tracking;
            current.Dispatch.DelayAlert = request.Dispatch.DelayAlert;
        }

        if (request.Notifications != null)
        {
            current.Notifications.Email = request.Notifications.Email;
            current.Notifications.System = request.Notifications.System;
            current.Notifications.Critical = request.Notifications.Critical;
            current.Notifications.LowStock = request.Notifications.LowStock;
            current.Notifications.Dispatch = request.Notifications.Dispatch;
            current.Notifications.Weekly = request.Notifications.Weekly;
        }

        if (request.Security != null)
        {
            current.Security.TwoFactor = request.Security.TwoFactor;
            current.Security.SessionTimeout = request.Security.SessionTimeout;
            current.Security.LoginAlerts = request.Security.LoginAlerts;
        }

        var updated = await _repository.UpdateSettingsAsync(current);
        var dto = MapToDto(updated);
        return ApiResponse<ResourceManagerSettingsDto>.SuccessResponse(dto, "Settings updated successfully");
    }

    public async Task<ApiResponse<ResourceManagerSettingsDto>> ResetSettingsAsync()
    {
        var reset = await _repository.ResetSettingsAsync();
        var dto = MapToDto(reset);
        return ApiResponse<ResourceManagerSettingsDto>.SuccessResponse(dto, "Settings reset to defaults successfully");
    }

    public async Task<ApiResponse<ProfileSettingsDto>> UpdateProfileAsync(ProfileSettingsDto dto)
    {
        var current = await _repository.GetSettingsAsync();
        current.Profile = new ProfileSettings
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Warehouse = dto.Warehouse
        };

        var updated = await _repository.UpdateSettingsAsync(current);
        return ApiResponse<ProfileSettingsDto>.SuccessResponse(MapProfileToDto(updated.Profile), "Profile updated successfully");
    }

    public async Task<ApiResponse<InventoryRulesSettingsDto>> UpdateInventoryRulesAsync(InventoryRulesSettingsDto dto)
    {
        var current = await _repository.GetSettingsAsync();
        current.Inventory = new InventoryRulesSettings
        {
            LowStock = dto.LowStock,
            CriticalStock = dto.CriticalStock,
            ExpiryDays = dto.ExpiryDays,
            AutoReorder = dto.AutoReorder,
            TrackExpiry = dto.TrackExpiry,
            AllowNegative = dto.AllowNegative
        };

        var updated = await _repository.UpdateSettingsAsync(current);
        return ApiResponse<InventoryRulesSettingsDto>.SuccessResponse(MapInventoryToDto(updated.Inventory), "Inventory rules updated successfully");
    }

    public async Task<ApiResponse<DispatchPreferencesSettingsDto>> UpdateDispatchPreferencesAsync(DispatchPreferencesSettingsDto dto)
    {
        var current = await _repository.GetSettingsAsync();
        current.Dispatch = new DispatchPreferencesSettings
        {
            DefaultVehicle = dto.DefaultVehicle,
            Approval = dto.Approval,
            Proof = dto.Proof,
            Tracking = dto.Tracking,
            DelayAlert = dto.DelayAlert
        };

        var updated = await _repository.UpdateSettingsAsync(current);
        return ApiResponse<DispatchPreferencesSettingsDto>.SuccessResponse(MapDispatchToDto(updated.Dispatch), "Dispatch preferences updated successfully");
    }

    public async Task<ApiResponse<NotificationPreferencesSettingsDto>> UpdateNotificationPreferencesAsync(NotificationPreferencesSettingsDto dto)
    {
        var current = await _repository.GetSettingsAsync();
        current.Notifications = new NotificationPreferencesSettings
        {
            Email = dto.Email,
            System = dto.System,
            Critical = dto.Critical,
            LowStock = dto.LowStock,
            Dispatch = dto.Dispatch,
            Weekly = dto.Weekly
        };

        var updated = await _repository.UpdateSettingsAsync(current);
        return ApiResponse<NotificationPreferencesSettingsDto>.SuccessResponse(MapNotificationsToDto(updated.Notifications), "Notification preferences updated successfully");
    }

    public async Task<ApiResponse<SecuritySettingsDto>> UpdateSecuritySettingsAsync(SecuritySettingsDto dto)
    {
        var current = await _repository.GetSettingsAsync();
        current.Security = new SecuritySettings
        {
            TwoFactor = dto.TwoFactor,
            SessionTimeout = dto.SessionTimeout,
            LoginAlerts = dto.LoginAlerts
        };

        var updated = await _repository.UpdateSettingsAsync(current);
        return ApiResponse<SecuritySettingsDto>.SuccessResponse(MapSecurityToDto(updated.Security), "Security settings updated successfully");
    }

    private static ResourceManagerSettingsDto MapToDto(ResourceManagerSettings entity)
    {
        return new ResourceManagerSettingsDto
        {
            Profile = MapProfileToDto(entity.Profile),
            Inventory = MapInventoryToDto(entity.Inventory),
            Dispatch = MapDispatchToDto(entity.Dispatch),
            Notifications = MapNotificationsToDto(entity.Notifications),
            Security = MapSecurityToDto(entity.Security),
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static ProfileSettingsDto MapProfileToDto(ProfileSettings profile)
    {
        return new ProfileSettingsDto
        {
            FullName = profile.FullName,
            Email = profile.Email,
            Phone = profile.Phone,
            Warehouse = profile.Warehouse
        };
    }

    private static InventoryRulesSettingsDto MapInventoryToDto(InventoryRulesSettings inventory)
    {
        return new InventoryRulesSettingsDto
        {
            LowStock = inventory.LowStock,
            CriticalStock = inventory.CriticalStock,
            ExpiryDays = inventory.ExpiryDays,
            AutoReorder = inventory.AutoReorder,
            TrackExpiry = inventory.TrackExpiry,
            AllowNegative = inventory.AllowNegative
        };
    }

    private static DispatchPreferencesSettingsDto MapDispatchToDto(DispatchPreferencesSettings dispatch)
    {
        return new DispatchPreferencesSettingsDto
        {
            DefaultVehicle = dispatch.DefaultVehicle,
            Approval = dispatch.Approval,
            Proof = dispatch.Proof,
            Tracking = dispatch.Tracking,
            DelayAlert = dispatch.DelayAlert
        };
    }

    private static NotificationPreferencesSettingsDto MapNotificationsToDto(NotificationPreferencesSettings notifications)
    {
        return new NotificationPreferencesSettingsDto
        {
            Email = notifications.Email,
            System = notifications.System,
            Critical = notifications.Critical,
            LowStock = notifications.LowStock,
            Dispatch = notifications.Dispatch,
            Weekly = notifications.Weekly
        };
    }

    private static SecuritySettingsDto MapSecurityToDto(SecuritySettings security)
    {
        return new SecuritySettingsDto
        {
            TwoFactor = security.TwoFactor,
            SessionTimeout = security.SessionTimeout,
            LoginAlerts = security.LoginAlerts
        };
    }
}
