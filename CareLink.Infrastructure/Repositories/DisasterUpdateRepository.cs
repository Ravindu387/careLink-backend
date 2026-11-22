using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class DisasterUpdateRepository : IDisasterUpdateRepository
{
    private readonly CareLinkDbContext _context;

    public DisasterUpdateRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DisasterUpdate>> GetAllAsync(
        DisasterUpdateQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DisasterUpdates.AsNoTracking().AsQueryable();

        // Archived filter
        if (parameters.Archived.HasValue)
        {
            query = query.Where(x => x.Archived == parameters.Archived.Value);
        }

        // Draft filter
        if (parameters.IsDraft.HasValue)
        {
            query = query.Where(x => x.IsDraft == parameters.IsDraft.Value);
        }

        // Type filter (e.g. Critical, Warning, Announcement, Update)
        if (!string.IsNullOrWhiteSpace(parameters.Type) &&
            !parameters.Type.Equals("All updates", StringComparison.OrdinalIgnoreCase))
        {
            var type = parameters.Type.Trim();
            // In frontend tabs: "Warnings" maps to "Warning", "Announcements" maps to "Announcement"
            if (type.Equals("Warnings", StringComparison.OrdinalIgnoreCase))
                type = "Warning";
            else if (type.Equals("Announcements", StringComparison.OrdinalIgnoreCase))
                type = "Announcement";

            query = query.Where(x => x.Type.ToLower() == type.ToLower());
        }

        // Search text
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var term = parameters.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Id.ToLower().Contains(term) ||
                x.Title.ToLower().Contains(term) ||
                x.Description.ToLower().Contains(term) ||
                x.Location.ToLower().Contains(term) ||
                x.Source.ToLower().Contains(term));
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<DisasterUpdate?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.DisasterUpdates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<DisasterUpdate> AddAsync(DisasterUpdate update, CancellationToken cancellationToken = default)
    {
        await _context.DisasterUpdates.AddAsync(update, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return update;
    }

    public async Task<bool> UpdateAsync(DisasterUpdate update, CancellationToken cancellationToken = default)
    {
        var existing = await _context.DisasterUpdates.FirstOrDefaultAsync(x => x.Id == update.Id, cancellationToken);
        if (existing == null) return false;

        existing.Title = update.Title;
        existing.Description = update.Description;
        existing.Type = update.Type;
        existing.Source = update.Source;
        existing.Location = update.Location;
        existing.Instructions = update.Instructions;
        existing.Archived = update.Archived;
        existing.IsDraft = update.IsDraft;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ToggleArchiveAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.DisasterUpdates.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (existing == null) return false;

        existing.Archived = !existing.Archived;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.DisasterUpdates.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (existing == null) return false;

        _context.DisasterUpdates.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<EmergencyContact>> GetEmergencyContactsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmergencyContacts
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }
}
