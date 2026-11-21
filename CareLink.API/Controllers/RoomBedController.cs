using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomBedController : ControllerBase
{
    private readonly IRoomBedService _roomBedService;
    private readonly ILogger<RoomBedController> _logger;

    public RoomBedController(
        IRoomBedService roomBedService,
        ILogger<RoomBedController> logger)
    {
        _roomBedService = roomBedService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all rooms with beds matching optional filters.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms(
        [FromQuery] RoomQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var rooms = await _roomBedService.GetRoomsAsync(parameters, cancellationToken);
        return Ok(rooms);
    }

    /// <summary>
    /// Gets overall room and bed summary KPI metrics.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<RoomBedSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _roomBedService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets a single room by its ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RoomDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var room = await _roomBedService.GetRoomByIdAsync(id, cancellationToken);
        if (room == null)
        {
            return NotFound(new { message = $"Room with ID '{id}' was not found." });
        }
        return Ok(room);
    }

    /// <summary>
    /// Adds a new room with an initial bed count.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RoomDto>> CreateRoom(
        [FromBody] CreateRoomDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _roomBedService.CreateRoomAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating room");
            return StatusCode(500, new { message = "An error occurred while creating the room." });
        }
    }

    /// <summary>
    /// Adds additional beds to an existing room.
    /// </summary>
    [HttpPost("beds")]
    public async Task<ActionResult<RoomDto>> AddBeds(
        [FromBody] AddBedsDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _roomBedService.AddBedsAsync(dto, cancellationToken);
            if (updated == null)
            {
                return NotFound(new { message = $"Room with ID '{dto.RoomId}' was not found." });
            }
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding beds to room {RoomId}", dto.RoomId);
            return StatusCode(500, new { message = "An error occurred while adding beds." });
        }
    }

    /// <summary>
    /// Updates the status and assigned resident of a bed.
    /// </summary>
    [HttpPatch("{roomId}/beds/{bedId}/status")]
    public async Task<ActionResult<RoomDto>> UpdateBedStatus(
        string roomId,
        string bedId,
        [FromBody] UpdateBedStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _roomBedService.UpdateBedStatusAsync(roomId, bedId, dto, cancellationToken);
            if (updated == null)
            {
                return NotFound(new { message = $"Bed '{bedId}' in room '{roomId}' was not found." });
            }
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for bed {BedId} in room {RoomId}", bedId, roomId);
            return StatusCode(500, new { message = "An error occurred while updating the bed status." });
        }
    }

    /// <summary>
    /// Deletes a room if it has no occupied or reserved beds.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _roomBedService.DeleteRoomAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound(new { message = $"Room with ID '{id}' was not found." });
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting room {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the room." });
        }
    }
}
