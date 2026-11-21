using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly CareLinkDbContext _context;

    public SettingsRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<UserSettings?> GetAsync()
    {
        return await _context.UserSettings.FirstOrDefaultAsync();
    }

    public async Task<UserSettings> UpsertAsync(UserSettings settings)
    {
        var existing = await _context.UserSettings.FindAsync(settings.Id);
        if (existing == null)
        {
            _context.UserSettings.Add(settings);
        }
        else
        {
            existing.FullName = settings.FullName;
            existing.Email = settings.Email;
            existing.Phone = settings.Phone;
            existing.Role = settings.Role;
            existing.ShelterName = settings.ShelterName;
            existing.ShelterLocation = settings.ShelterLocation;
            existing.ShelterCapacity = settings.ShelterCapacity;
            existing.ShelterManager = settings.ShelterManager;
            existing.ShelterAddress = settings.ShelterAddress;
            existing.TwoFactor = settings.TwoFactor;
            existing.LoginAlerts = settings.LoginAlerts;
            existing.NotifInApp = settings.NotifInApp;
            existing.NotifEmail = settings.NotifEmail;
            existing.NotifSms = settings.NotifSms;
            existing.NotifCritical = settings.NotifCritical;
            existing.NotifRequests = settings.NotifRequests;
            existing.NotifInventory = settings.NotifInventory;
            existing.NotifSystem = settings.NotifSystem;
            _context.UserSettings.Update(existing);
        }
        await _context.SaveChangesAsync();
        return existing ?? settings;
    }
}
