using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _repo;

    public SettingsService(ISettingsRepository repo)
    {
        _repo = repo;
    }

    private async Task<UserSettings> GetOrCreateAsync()
    {
        var settings = await _repo.GetAsync();
        if (settings != null) return settings;

        // Seed defaults if not found
        var defaults = new UserSettings
        {
            Id = "default",
            FullName = "Sanjana Amarasinghe",
            Email = "sanjana@carelink.org",
            Phone = "+94 77 321 8490",
            Role = "Shelter Administrator",
            ShelterName = "Riverton Central School",
            ShelterLocation = "Central District",
            ShelterCapacity = "660",
            ShelterManager = "Sanjana Amarasinghe",
            ShelterAddress = "Riverton, Greater Riverton",
            TwoFactor = true,
            LoginAlerts = true,
            NotifInApp = true,
            NotifEmail = true,
            NotifSms = false,
            NotifCritical = true,
            NotifRequests = true,
            NotifInventory = true,
            NotifSystem = false,
        };
        return await _repo.UpsertAsync(defaults);
    }

    public async Task<ProfileDto> GetProfileAsync()
    {
        var s = await GetOrCreateAsync();
        return new ProfileDto
        {
            FullName = s.FullName,
            Email = s.Email,
            Phone = s.Phone,
            Role = s.Role,
        };
    }

    public async Task<ProfileDto> UpdateProfileAsync(ProfileDto dto)
    {
        var s = await GetOrCreateAsync();
        s.FullName = dto.FullName;
        s.Email = dto.Email;
        s.Phone = dto.Phone;
        // Role is read-only — do not update
        await _repo.UpsertAsync(s);
        return new ProfileDto
        {
            FullName = s.FullName,
            Email = s.Email,
            Phone = s.Phone,
            Role = s.Role,
        };
    }

    public async Task<ShelterSettingsDto> GetShelterSettingsAsync()
    {
        var s = await GetOrCreateAsync();
        return new ShelterSettingsDto
        {
            Name = s.ShelterName,
            Location = s.ShelterLocation,
            Capacity = s.ShelterCapacity,
            Manager = s.ShelterManager,
            Address = s.ShelterAddress,
        };
    }

    public async Task<ShelterSettingsDto> UpdateShelterSettingsAsync(ShelterSettingsDto dto)
    {
        var s = await GetOrCreateAsync();
        s.ShelterName = dto.Name;
        s.ShelterLocation = dto.Location;
        s.ShelterCapacity = dto.Capacity;
        s.ShelterManager = dto.Manager;
        s.ShelterAddress = dto.Address;
        await _repo.UpsertAsync(s);
        return new ShelterSettingsDto
        {
            Name = s.ShelterName,
            Location = s.ShelterLocation,
            Capacity = s.ShelterCapacity,
            Manager = s.ShelterManager,
            Address = s.ShelterAddress,
        };
    }

    public async Task<SecuritySettingsDto> GetSecuritySettingsAsync()
    {
        var s = await GetOrCreateAsync();
        return new SecuritySettingsDto
        {
            TwoFactor = s.TwoFactor,
            LoginAlerts = s.LoginAlerts,
        };
    }

    public async Task<bool> UpdateSecuritySettingsAsync(SecuritySettingsDto dto)
    {
        var s = await GetOrCreateAsync();
        s.TwoFactor = dto.TwoFactor;
        s.LoginAlerts = dto.LoginAlerts;
        await _repo.UpsertAsync(s);
        return true;
    }

    public async Task<NotificationSettingsDto> GetNotificationSettingsAsync()
    {
        var s = await GetOrCreateAsync();
        return new NotificationSettingsDto
        {
            InApp = s.NotifInApp,
            Email = s.NotifEmail,
            Sms = s.NotifSms,
            Critical = s.NotifCritical,
            Requests = s.NotifRequests,
            Inventory = s.NotifInventory,
            System = s.NotifSystem,
        };
    }

    public async Task<NotificationSettingsDto> UpdateNotificationSettingsAsync(NotificationSettingsDto dto)
    {
        var s = await GetOrCreateAsync();
        s.NotifInApp = dto.InApp;
        s.NotifEmail = dto.Email;
        s.NotifSms = dto.Sms;
        s.NotifCritical = dto.Critical;
        s.NotifRequests = dto.Requests;
        s.NotifInventory = dto.Inventory;
        s.NotifSystem = dto.System;
        await _repo.UpsertAsync(s);
        return new NotificationSettingsDto
        {
            InApp = s.NotifInApp,
            Email = s.NotifEmail,
            Sms = s.NotifSms,
            Critical = s.NotifCritical,
            Requests = s.NotifRequests,
            Inventory = s.NotifInventory,
            System = s.NotifSystem,
        };
    }
}
