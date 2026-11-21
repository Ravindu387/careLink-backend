using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class RoomBedRepository : IRoomBedRepository
{
    private readonly CareLinkDbContext _context;

    public RoomBedRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Room>> GetRoomsAsync(
        RoomQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Rooms
            .Include(r => r.Beds)
            .AsNoTracking()
            .AsQueryable();

        // Shelter filter
        if (!string.IsNullOrWhiteSpace(parameters.Shelter) &&
            !parameters.Shelter.Equals("All shelters", StringComparison.OrdinalIgnoreCase))
        {
            var shelter = parameters.Shelter.Trim().ToLower();
            query = query.Where(r =>
                r.ShelterId.ToLower() == shelter ||
                r.ShelterName.ToLower().Contains(shelter));
        }

        // Floor filter
        if (!string.IsNullOrWhiteSpace(parameters.Floor) &&
            !parameters.Floor.Equals("All floors", StringComparison.OrdinalIgnoreCase))
        {
            var floor = parameters.Floor.Trim().ToLower();
            query = query.Where(r => r.Floor.ToLower() == floor);
        }

        // Room type filter
        if (!string.IsNullOrWhiteSpace(parameters.Type) &&
            !parameters.Type.Equals("All room types", StringComparison.OrdinalIgnoreCase))
        {
            var type = parameters.Type.Trim().ToLower();
            query = query.Where(r => r.Type.ToLower() == type);
        }

        // Search text
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLower();
            query = query.Where(r =>
                r.Name.ToLower().Contains(search) ||
                r.ShelterName.ToLower().Contains(search) ||
                r.Id.ToLower().Contains(search));
        }

        return await query
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Room?> GetRoomByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Rooms
            .Include(r => r.Beds)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<(int TotalRooms, int TotalBeds, int AvailableBeds, int OccupiedBeds, int ReservedBeds, int UnavailableBeds)> GetSummaryMetricsAsync(CancellationToken cancellationToken = default)
    {
        var totalRooms = await _context.Rooms.CountAsync(cancellationToken);
        var beds = await _context.Beds.AsNoTracking().ToListAsync(cancellationToken);

        int totalBeds = beds.Count;
        int available = beds.Count(b => b.Status.Equals("Available", StringComparison.OrdinalIgnoreCase));
        int occupied = beds.Count(b => b.Status.Equals("Occupied", StringComparison.OrdinalIgnoreCase));
        int reserved = beds.Count(b => b.Status.Equals("Reserved", StringComparison.OrdinalIgnoreCase));
        int unavailable = beds.Count(b => b.Status.Equals("Unavailable", StringComparison.OrdinalIgnoreCase));

        return (totalRooms, totalBeds, available, occupied, reserved, unavailable);
    }

    public async Task<Room> AddRoomAsync(Room room, CancellationToken cancellationToken = default)
    {
        await _context.Rooms.AddAsync(room, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return room;
    }

    public async Task<Room?> UpdateRoomAsync(Room room, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync(cancellationToken);
        return room;
    }

    public async Task<bool> DeleteRoomAsync(string id, CancellationToken cancellationToken = default)
    {
        var room = await _context.Rooms
            .Include(r => r.Beds)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (room == null) return false;

        _context.Beds.RemoveRange(room.Beds);
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<Room?> AddBedsAsync(string roomId, int count, CancellationToken cancellationToken = default)
    {
        var room = await _context.Rooms
            .Include(r => r.Beds)
            .FirstOrDefaultAsync(r => r.Id == roomId, cancellationToken);

        if (room == null) return null;

        var existingNumbers = room.Beds
            .Select(b =>
            {
                var parts = b.BedNumber.Split('-');
                if (parts.Length > 1 && int.TryParse(parts[1], out var n)) return n;
                return 0;
            })
            .ToList();

        var lastNumber = existingNumbers.Count > 0 ? existingNumbers.Max() : 0;

        for (int i = 1; i <= count; i++)
        {
            var bedNum = $"B-{lastNumber + i:D2}";
            var newBed = new Bed
            {
                Id = $"{roomId}-{bedNum}",
                BedNumber = bedNum,
                RoomId = roomId,
                Status = "Available",
                UpdatedAt = DateTime.UtcNow
            };
            room.Beds.Add(newBed);
            await _context.Beds.AddAsync(newBed, cancellationToken);
        }

        room.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return room;
    }

    public async Task<bool> UpdateBedStatusAsync(
        string roomId,
        string bedId,
        string status,
        string? resident,
        CancellationToken cancellationToken = default)
    {
        var bed = await _context.Beds
            .FirstOrDefaultAsync(b =>
                b.RoomId == roomId &&
                (b.BedNumber.ToLower() == bedId.ToLower() || b.Id.ToLower() == bedId.ToLower()),
                cancellationToken);

        if (bed == null) return false;

        bed.Status = status;
        if (status.Equals("Occupied", StringComparison.OrdinalIgnoreCase))
        {
            bed.ResidentName = !string.IsNullOrWhiteSpace(resident) ? resident : "New resident";
        }
        else
        {
            bed.ResidentName = null;
            bed.ResidentId = null;
        }

        bed.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
