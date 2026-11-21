using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class ShelterRepository : IShelterRepository
{
    private readonly CareLinkDbContext _context;

    public ShelterRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Shelter>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Shelters
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Shelter?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Shelters
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Shelter> AddAsync(Shelter shelter, CancellationToken cancellationToken = default)
    {
        await _context.Shelters.AddAsync(shelter, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return shelter;
    }

    public async Task<(IEnumerable<Shelter> Items, int TotalCount)> GetPagedAsync(
        ShelterQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Shelters.AsNoTracking().AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(search) ||
                s.Id.ToLower().Contains(search) ||
                s.Manager.ToLower().Contains(search));
        }

        // District filter
        if (!string.IsNullOrWhiteSpace(parameters.District) &&
            !parameters.District.Equals("All districts", StringComparison.OrdinalIgnoreCase))
        {
            var district = parameters.District.Trim().ToLower();
            query = query.Where(s => s.District.ToLower() == district);
        }

        // Type filter
        if (!string.IsNullOrWhiteSpace(parameters.Type) &&
            !parameters.Type.Equals("All shelter types", StringComparison.OrdinalIgnoreCase))
        {
            var type = parameters.Type.Trim().ToLower();
            query = query.Where(s => s.Type.ToLower() == type);
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            !parameters.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            var status = parameters.Status.Trim().ToLower();
            query = query.Where(s => s.Status.ToLower() == status);
        }

        // Occupancy filter
        if (!string.IsNullOrWhiteSpace(parameters.Occupancy) &&
            !parameters.Occupancy.Equals("Any occupancy", StringComparison.OrdinalIgnoreCase))
        {
            var occ = parameters.Occupancy.Trim().ToLower();
            if (occ.Contains("available"))
            {
                query = query.Where(s => s.Capacity - s.CurrentOccupancy > 0);
            }
            else if (occ.Contains("near capacity"))
            {
                query = query.Where(s => s.Status.ToLower() == "near capacity");
            }
            else if (occ.Contains("fully occupied"))
            {
                query = query.Where(s => s.CurrentOccupancy >= s.Capacity);
            }
            else if (occ.Contains("empty"))
            {
                query = query.Where(s => s.CurrentOccupancy == 0);
            }
        }

        // Capacity bounds
        if (parameters.MinCapacity.HasValue)
        {
            query = query.Where(s => s.Capacity >= parameters.MinCapacity.Value);
        }

        if (parameters.MaxCapacity.HasValue)
        {
            query = query.Where(s => s.Capacity <= parameters.MaxCapacity.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = parameters.Page.GetValueOrDefault(1);
        if (page < 1) page = 1;
        var pageSize = parameters.PageSize.GetValueOrDefault(10);
        if (pageSize < 1) pageSize = 10;

        var items = await query
            .OrderBy(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(int Registered, int Operational, int NearCapacity, int Maintenance)> GetSummaryMetricsAsync(CancellationToken cancellationToken = default)
    {
        var shelters = await _context.Shelters.AsNoTracking().ToListAsync(cancellationToken);

        var registered = shelters.Count;
        var operational = shelters.Count(s => s.Status == "Active" || s.Status == "Near Capacity");
        var nearCapacity = shelters.Count(s => s.Status == "Near Capacity");
        var maintenance = shelters.Count(s => s.Status == "Maintenance");

        return (registered, operational, nearCapacity, maintenance);
    }
}
