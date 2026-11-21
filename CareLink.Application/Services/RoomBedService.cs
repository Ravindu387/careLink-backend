using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class RoomBedService : IRoomBedService
{
    private readonly IRoomBedRepository _roomBedRepository;
    private readonly IShelterRepository _shelterRepository;

    public RoomBedService(
        IRoomBedRepository roomBedRepository,
        IShelterRepository shelterRepository)
    {
        _roomBedRepository = roomBedRepository;
        _shelterRepository = shelterRepository;
    }

    public async Task<IEnumerable<RoomDto>> GetRoomsAsync(
        RoomQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var items = await _roomBedRepository.GetRoomsAsync(parameters, cancellationToken);
        return items.Select(MapToDto);
    }

    public async Task<RoomDto?> GetRoomByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var room = await _roomBedRepository.GetRoomByIdAsync(id, cancellationToken);
        return room != null ? MapToDto(room) : null;
    }

    public async Task<RoomBedSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var metrics = await _roomBedRepository.GetSummaryMetricsAsync(cancellationToken);

        return new RoomBedSummaryDto
        {
            TotalRooms = metrics.TotalRooms,
            TotalBeds = metrics.TotalBeds,
            AvailableBeds = metrics.AvailableBeds,
            OccupiedBeds = metrics.OccupiedBeds,
            ReservedBeds = metrics.ReservedBeds,
            UnavailableBeds = metrics.UnavailableBeds
        };
    }

    public async Task<RoomDto> CreateRoomAsync(
        CreateRoomDto dto,
        CancellationToken cancellationToken = default)
    {
        string shelterId = dto.Shelter;
        string shelterName = dto.Shelter;

        var allShelters = await _shelterRepository.GetAllAsync(cancellationToken);
        var matched = allShelters.FirstOrDefault(s =>
            s.Id.Equals(dto.Shelter, StringComparison.OrdinalIgnoreCase) ||
            s.Name.Equals(dto.Shelter, StringComparison.OrdinalIgnoreCase));

        if (matched != null)
        {
            shelterId = matched.Id;
            shelterName = matched.Name;
        }

        var roomId = $"RM-{Random.Shared.Next(100, 999)}";
        var count = Math.Clamp(dto.BedCount, 1, 100);

        var room = new Room
        {
            Id = roomId,
            Name = dto.Name.Trim(),
            ShelterId = shelterId,
            ShelterName = shelterName,
            Floor = dto.Floor.Trim(),
            Type = dto.Type.Trim(),
            CreatedAt = DateTime.UtcNow,
            Beds = Enumerable.Range(1, count).Select(i =>
            {
                var bedNum = $"B-{i:D2}";
                return new Bed
                {
                    Id = $"{roomId}-{bedNum}",
                    BedNumber = bedNum,
                    RoomId = roomId,
                    Status = "Available",
                    UpdatedAt = DateTime.UtcNow
                };
            }).ToList()
        };

        var created = await _roomBedRepository.AddRoomAsync(room, cancellationToken);
        return MapToDto(created);
    }

    public async Task<RoomDto?> AddBedsAsync(
        AddBedsDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.Count <= 0)
        {
            throw new InvalidOperationException("Number of beds to add must be greater than zero.");
        }

        var updated = await _roomBedRepository.AddBedsAsync(dto.RoomId, dto.Count, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<RoomDto?> UpdateBedStatusAsync(
        string roomId,
        string bedId,
        UpdateBedStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var success = await _roomBedRepository.UpdateBedStatusAsync(roomId, bedId, dto.Status.Trim(), dto.Resident?.Trim(), cancellationToken);
        if (!success) return null;

        var room = await _roomBedRepository.GetRoomByIdAsync(roomId, cancellationToken);
        return room != null ? MapToDto(room) : null;
    }

    public async Task<bool> DeleteRoomAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _roomBedRepository.GetRoomByIdAsync(id, cancellationToken);
        if (existing == null) return false;

        if (existing.Beds.Any(b => b.Status == "Occupied" || b.Status == "Reserved"))
        {
            throw new InvalidOperationException("Cannot delete a room containing occupied or reserved beds.");
        }

        return await _roomBedRepository.DeleteRoomAsync(id, cancellationToken);
    }

    private static RoomDto MapToDto(Room room)
    {
        return new RoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Shelter = room.ShelterName,
            ShelterId = room.ShelterId,
            Floor = room.Floor,
            Type = room.Type,
            Beds = room.Beds
                .OrderBy(b => b.BedNumber)
                .Select(b => new BedDto
                {
                    Id = b.BedNumber,
                    Status = b.Status,
                    Resident = b.ResidentName
                }).ToList()
        };
    }
}
