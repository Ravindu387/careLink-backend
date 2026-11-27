using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/maintenance-requests")]
public class MaintenanceRequestsController : ControllerBase
{
    private readonly IMaintenanceRequestService _service;
    private readonly ILogger<MaintenanceRequestsController> _logger;

    public MaintenanceRequestsController(
        IMaintenanceRequestService service,
        ILogger<MaintenanceRequestsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Gets maintenance requests with search, filtering, and pagination.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<MaintenanceRequestDto>>> GetPaged(
        [FromQuery] MaintenanceRequestQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedRequestsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets aggregated summary counts for maintenance requests.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<MaintenanceRequestSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _service.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets categories, technicians, shelters, priorities, and statuses metadata.
    /// </summary>
    [HttpGet("meta")]
    public async Task<ActionResult<MaintenanceRequestMetaDto>> GetMeta(CancellationToken cancellationToken)
    {
        var meta = await _service.GetMetaAsync(cancellationToken);
        return Ok(meta);
    }

    /// <summary>
    /// Gets a single maintenance request by its ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MaintenanceRequestDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var item = await _service.GetRequestByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { message = $"Maintenance request with ID '{id}' was not found." });
        }
        return Ok(item);
    }

    /// <summary>
    /// Creates a new maintenance request.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MaintenanceRequestDto>> Create(
        [FromBody] CreateMaintenanceRequestDto dto,
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
            _logger.LogError(ex, "Error creating maintenance request");
            return StatusCode(500, new { message = "An error occurred while creating the maintenance request." });
        }
    }

    /// <summary>
    /// Partially updates an existing maintenance request.
    /// </summary>
    [HttpPatch("{id}")]
    [HttpPut("{id}")]
    public async Task<ActionResult<MaintenanceRequestDto>> Update(
        string id,
        [FromBody] UpdateMaintenanceRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _service.UpdateRequestAsync(id, dto, cancellationToken);
            if (updated == null)
            {
                return NotFound(new { message = $"Maintenance request with ID '{id}' was not found." });
            }

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating maintenance request {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the maintenance request." });
        }
    }

    /// <summary>
    /// Deletes a maintenance request by its ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken)
    {
        var success = await _service.DeleteRequestAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = $"Maintenance request with ID '{id}' was not found." });
        }

        return NoContent();
    }
}
