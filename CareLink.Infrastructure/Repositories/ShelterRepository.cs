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

    public async Task<IReadOnlyList<Shelter>> GetAllAsync(
        ShelterFilterParams? filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Shelters.AsNoTracking().AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim().ToLowerInvariant();
                query = query.Where(item =>
                    item.Name.ToLower().Contains(s) ||
                    item.Location.ToLower().Contains(s) ||
                    item.District.ToLower().Contains(s) ||
                    (item.Notes != null && item.Notes.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Area) &&
                !filter.Area.Equals("All operational areas", StringComparison.OrdinalIgnoreCase) &&
                !filter.Area.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(item => item.Location.ToLower() == filter.Area.Trim().ToLower() || item.District.ToLower() == filter.Area.Trim().ToLower());
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                !filter.Status.Equals("All statuses", StringComparison.OrdinalIgnoreCase) &&
                !filter.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(item => item.Status.ToLower() == filter.Status.Trim().ToLower());
            }

            if (!string.IsNullOrWhiteSpace(filter.Availability) &&
                !filter.Availability.Equals("Any availability", StringComparison.OrdinalIgnoreCase) &&
                !filter.Availability.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (filter.Availability.Equals("Available", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(item => item.Capacity - item.CurrentOccupancy > 0);
                }
                else if (filter.Availability.Equals("Full", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(item => item.Capacity - item.CurrentOccupancy <= 0);
                }
            }
        }

        var result = await query.OrderBy(item => item.Id).ToListAsync(cancellationToken);
        return result;
    }

    public async Task<Shelter?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Shelters
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Shelter?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var idStr = id.ToString();
        var code = $"SH-{id:D3}";
        return await _context.Shelters
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == idStr || s.Id == code, cancellationToken);
    }

    public async Task<Shelter> AddAsync(Shelter shelter, CancellationToken cancellationToken = default)
    {
        await _context.Shelters.AddAsync(shelter, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return shelter;
    }

    public async Task<Shelter?> UpdateAsync(Shelter shelter, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Shelters.FirstOrDefaultAsync(s => s.Id == shelter.Id, cancellationToken);
        if (existing == null) return null;

        existing.Name = shelter.Name;
        existing.Location = shelter.Location;
        existing.District = shelter.District;
        existing.Type = shelter.Type;
        existing.Manager = shelter.Manager;
        existing.Status = shelter.Status;
        existing.Capacity = shelter.Capacity;
        existing.CurrentOccupancy = shelter.CurrentOccupancy;
        existing.Reserved = shelter.Reserved;
        existing.Services = shelter.Services;
        existing.ContactPhone = shelter.ContactPhone;
        existing.Notes = shelter.Notes;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<Shelter?> UpdateOccupancyAsync(
        int id,
        int occupied,
        CancellationToken cancellationToken = default)
    {
        var idStr = id.ToString();
        var code = $"SH-{id:D3}";
        var existing = await _context.Shelters.FirstOrDefaultAsync(s => s.Id == idStr || s.Id == code, cancellationToken);
        if (existing == null) return null;

        existing.CurrentOccupancy = Math.Clamp(occupied, 0, existing.Capacity);
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        if (existing.CurrentOccupancy >= existing.Capacity)
            existing.Status = "Full";
        else if (existing.CurrentOccupancy >= (int)(existing.Capacity * 0.9))
            existing.Status = "Near Capacity";
        else
            existing.Status = "Active";

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var idStr = id.ToString();
        var code = $"SH-{id:D3}";
        var existing = await _context.Shelters.FirstOrDefaultAsync(s => s.Id == idStr || s.Id == code, cancellationToken);
        if (existing == null) return false;

        _context.Shelters.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Shelters.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (existing == null) return false;

        _context.Shelters.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ShelterMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var shelters = await _context.Shelters.AsNoTracking().ToListAsync(cancellationToken);

        int totalCap = shelters.Sum(s => s.Capacity);
        int totalOcc = shelters.Sum(s => s.CurrentOccupancy);
        int avail = Math.Max(0, totalCap - totalOcc);
        int operational = shelters.Count(s => s.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) || s.Status.Equals("Operational", StringComparison.OrdinalIgnoreCase));
        int nearCap = shelters.Count(s => s.Status.Equals("Near Capacity", StringComparison.OrdinalIgnoreCase));
        int full = shelters.Count(s => s.Status.Equals("Full", StringComparison.OrdinalIgnoreCase));
        int accepting = shelters.Count(s => (s.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) || s.Status.Equals("Operational", StringComparison.OrdinalIgnoreCase)) && s.Capacity > s.CurrentOccupancy);

        return new ShelterMetrics
        {
            OperationalShelters = operational,
            TotalCapacity = totalCap,
            TotalOccupied = totalOcc,
            AvailableSpaces = avail,
            AcceptingArrivals = accepting,
            NearCapacityCount = nearCap,
            FullCount = full,
            TotalShelters = shelters.Count,
            LastUpdated = DateTimeOffset.UtcNow
        };
    }

    public async Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        _context.Shelters.RemoveRange(_context.Shelters);
        await _context.SaveChangesAsync(cancellationToken);
        await DataSeeder.SeedAsync(_context);
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
        var operational = shelters.Count(s => s.Status == "Active" || s.Status == "Near Capacity" || s.Status == "Operational");
        var nearCapacity = shelters.Count(s => s.Status == "Near Capacity");
        var maintenance = shelters.Count(s => s.Status == "Maintenance");

        return (registered, operational, nearCapacity, maintenance);
    }

    #region Citizen Portal & Extended Shelter Management

    private static readonly List<ShelterBooking> _bookings = new();
    private static readonly object _bookingLock = new();
    private static int _bookingCounter = 1001;

    public async Task<IEnumerable<Shelter>> GetSheltersAsync(
        string? search = null,
        string? area = null,
        double? maxDistance = null,
        string? availability = null,
        bool? accessibleOnly = null,
        string? service = null)
    {
        var query = _context.Shelters.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(term) ||
                s.Location.ToLower().Contains(term) ||
                s.District.ToLower().Contains(term) ||
                s.Address.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(area) && !area.Equals("All", StringComparison.OrdinalIgnoreCase) && !area.Equals("All areas", StringComparison.OrdinalIgnoreCase))
        {
            var a = area.Trim().ToLower();
            query = query.Where(s => s.Location.ToLower() == a || s.District.ToLower() == a);
        }

        if (maxDistance.HasValue)
        {
            query = query.Where(s => s.Distance <= maxDistance.Value);
        }

        if (!string.IsNullOrWhiteSpace(availability) && !availability.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            var av = availability.Trim().ToLower();
            query = query.Where(s => s.Status.ToLower() == av);
        }

        if (accessibleOnly == true)
        {
            query = query.Where(s => s.Accessible);
        }

        var results = await query.ToListAsync();

        if (!string.IsNullOrWhiteSpace(service) && !service.Equals("All services", StringComparison.OrdinalIgnoreCase))
        {
            var svcLower = service.Trim().ToLowerInvariant();
            results = results.Where(s => s.Services.Any(sv => sv.ToLowerInvariant().Contains(svcLower))).ToList();
        }

        return results.OrderBy(s => s.Distance);
    }

    public Task<Shelter> CreateAsync(Shelter shelter)
    {
        return AddAsync(shelter);
    }

    public Task<Shelter?> UpdateAsync(string id, Shelter shelter)
    {
        shelter.Id = id;
        return UpdateAsync(shelter);
    }

    public async Task<ShelterBooking> CreateBookingAsync(ShelterBooking booking)
    {
        lock (_bookingLock)
        {
            if (string.IsNullOrWhiteSpace(booking.Id))
            {
                booking.Id = $"SH-RES-{_bookingCounter++}";
            }
            booking.BookedAt = DateTime.UtcNow;
            _bookings.Add(booking);
        }

        var shelter = await _context.Shelters.FirstOrDefaultAsync(s => s.Id == booking.ShelterId);
        if (shelter == null && int.TryParse(booking.ShelterId, out var num))
        {
            var code = $"SH-{num:D3}";
            shelter = await _context.Shelters.FirstOrDefaultAsync(s => s.Id == code);
        }

        if (shelter != null)
        {
            shelter.CurrentOccupancy = Math.Min(shelter.Capacity, shelter.CurrentOccupancy + booking.NumberOfPeople);
            shelter.Status = shelter.Remaining > 0 ? "Space available" : "Full";
            shelter.UpdatedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();
        }

        return booking;
    }

    public Task<IEnumerable<ShelterBooking>> GetBookingsAsync(string? citizenName = null)
    {
        lock (_bookingLock)
        {
            var query = _bookings.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(citizenName))
            {
                query = query.Where(b => b.CitizenName.Equals(citizenName.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult<IEnumerable<ShelterBooking>>(query.OrderByDescending(b => b.BookedAt).ToList());
        }
    }

    #endregion
}
