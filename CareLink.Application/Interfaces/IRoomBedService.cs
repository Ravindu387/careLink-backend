using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IRoomBedService
{
    Task<IEnumerable<RoomDto>> GetRoomsAsync(RoomQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<RoomDto?> GetRoomByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<RoomBedSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<RoomDto> CreateRoomAsync(CreateRoomDto dto, CancellationToken cancellationToken = default);
    Task<RoomDto?> AddBedsAsync(AddBedsDto dto, CancellationToken cancellationToken = default);
    Task<RoomDto?> UpdateBedStatusAsync(string roomId, string bedId, UpdateBedStatusDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteRoomAsync(string id, CancellationToken cancellationToken = default);
}
