using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IRoomBedRepository
{
    Task<IEnumerable<Room>> GetRoomsAsync(RoomQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<Room?> GetRoomByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<(int TotalRooms, int TotalBeds, int AvailableBeds, int OccupiedBeds, int ReservedBeds, int UnavailableBeds)> GetSummaryMetricsAsync(CancellationToken cancellationToken = default);
    Task<Room> AddRoomAsync(Room room, CancellationToken cancellationToken = default);
    Task<Room?> UpdateRoomAsync(Room room, CancellationToken cancellationToken = default);
    Task<bool> DeleteRoomAsync(string id, CancellationToken cancellationToken = default);
    Task<Room?> AddBedsAsync(string roomId, int count, CancellationToken cancellationToken = default);
    Task<bool> UpdateBedStatusAsync(string roomId, string bedId, string status, string? resident, CancellationToken cancellationToken = default);
}
