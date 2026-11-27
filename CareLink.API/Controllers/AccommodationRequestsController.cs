using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/accommodation-requests")]
public class AccommodationRequestsController : ControllerBase
{
    private readonly IAccommodationRequestService _service;
    private readonly ILogger<AccommodationRequestsController> _logger;

    public AccommodationRequestsController(
        IAccommodationRequestService service,
        ILogger<AccommodationRequestsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Gets accommodation requests with search, filtering, and pagination.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<AccommodationRequestDto>>> GetPaged(
        [FromQuery] AccommodationRequestQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedRequestsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets aggregated status counts for the requests.
    /// </summary>
    [HttpGet("counts")]
    public async Task<ActionResult<AccommodationRequestCountsDto>> GetCounts(CancellationToken cancellationToken)
    {
        var counts = await _service.GetStatusCountsAsync(cancellationToken);
        return Ok(counts);
    }

    /// <summary>
    /// Gets an accommodation request by its ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AccommodationRequestDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var item = await _service.GetRequestByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { message = $"Accommodation request with ID '{id}' was not found." });
        }
        return Ok(item);
    }

    /// <summary>
    /// Creates a new accommodation request.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AccommodationRequestDto>> Create(
        [FromBody] CreateAccommodationRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _service.CreateRequestAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating accommodation request.");
            return StatusCode(500, new { message = "An error occurred while creating the accommodation request." });
        }
    }

    /// <summary>
    /// Updates the status of an accommodation request.
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<AccommodationRequestDto>> UpdateStatus(
        string id,
        [FromBody] UpdateAccommodationRequestStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateStatusAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Accommodation request with ID '{id}' was not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Deletes an accommodation request.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteRequestAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = $"Accommodation request with ID '{id}' was not found." });
        }

        return NoContent();
    }
}
