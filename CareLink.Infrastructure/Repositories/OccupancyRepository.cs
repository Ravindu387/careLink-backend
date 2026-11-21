using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class OccupancyRepository : IOccupancyRepository
{
    private readonly CareLinkDbContext _context;

    public OccupancyRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Shelter>> GetSheltersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Shelters
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Shelter?> GetShelterByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Shelters
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<bool> UpdateShelterAsync(Shelter shelter, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Shelters.FirstOrDefaultAsync(s => s.Id == shelter.Id, cancellationToken);
        if (existing == null) return false;

        if (!ReferenceEquals(existing, shelter))
        {
            _context.Entry(existing).CurrentValues.SetValues(shelter);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<Admission>> GetAdmissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Admissions
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Admission> AddAdmissionAsync(Admission admission, CancellationToken cancellationToken = default)
    {
        await _context.Admissions.AddAsync(admission, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return admission;
    }

    public async Task<OccupancyThreshold?> GetThresholdsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OccupancyThresholds
            .FirstOrDefaultAsync(t => t.Id == 1, cancellationToken);
    }

    public async Task<OccupancyThreshold> SaveThresholdsAsync(OccupancyThreshold thresholds, CancellationToken cancellationToken = default)
    {
        var existing = await _context.OccupancyThresholds
            .FirstOrDefaultAsync(t => t.Id == 1, cancellationToken);

        if (existing == null)
        {
            thresholds.Id = 1;
            await _context.OccupancyThresholds.AddAsync(thresholds, cancellationToken);
        }
        else
        {
            existing.Warning = thresholds.Warning;
            existing.Critical = thresholds.Critical;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return existing ?? thresholds;
    }
}
