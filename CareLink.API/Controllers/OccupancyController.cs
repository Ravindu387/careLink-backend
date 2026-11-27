using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/occupancy")]
public class OccupancyController : ControllerBase
{
    private readonly IOccupancyService _occupancyService;
    private readonly ILogger<OccupancyController> _logger;

    public OccupancyController(
        IOccupancyService occupancyService,
        ILogger<OccupancyController> logger)
    {
        _occupancyService = occupancyService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all shelters with live occupancy calculations and threshold statuses.
    /// </summary>
    [HttpGet("shelters")]
    public async Task<ActionResult<IEnumerable<OccupancyShelterDto>>> GetShelters(CancellationToken cancellationToken)
    {
        var shelters = await _occupancyService.GetOccupancySheltersAsync(cancellationToken);
        return Ok(shelters);
    }

    /// <summary>
    /// Gets overall occupancy metrics, capacities, percentages, and totals.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<OccupancySummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _occupancyService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets recent resident admissions.
    /// </summary>
    [HttpGet("admissions")]
    public async Task<ActionResult<IEnumerable<AdmissionDto>>> GetAdmissions(CancellationToken cancellationToken)
    {
        var admissions = await _occupancyService.GetAdmissionsAsync(cancellationToken);
        return Ok(admissions);
    }

    /// <summary>
    /// Records a new resident admission and updates the shelter's occupancy count.
    /// </summary>
    [HttpPost("admissions")]
    public async Task<ActionResult<AdmissionDto>> RecordAdmission(
        [FromBody] RecordAdmissionDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var admission = await _occupancyService.RecordAdmissionAsync(dto, cancellationToken);
            return Ok(admission);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording admission");
            return StatusCode(500, new { message = "An error occurred while recording admission." });
        }
    }

    /// <summary>
    /// Gets current warning and critical occupancy thresholds.
    /// </summary>
    [HttpGet("thresholds")]
    public async Task<ActionResult<OccupancyThresholdDto>> GetThresholds(CancellationToken cancellationToken)
    {
        var thresholds = await _occupancyService.GetThresholdsAsync(cancellationToken);
        return Ok(thresholds);
    }

    /// <summary>
    /// Updates warning and critical occupancy thresholds.
    /// </summary>
    [HttpPut("thresholds")]
    public async Task<ActionResult<OccupancyThresholdDto>> SaveThresholds(
        [FromBody] OccupancyThresholdDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _occupancyService.SaveThresholdsAsync(dto, cancellationToken);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating thresholds");
            return StatusCode(500, new { message = "An error occurred while saving thresholds." });
        }
    }
}
