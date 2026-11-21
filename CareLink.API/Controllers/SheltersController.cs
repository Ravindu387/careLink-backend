using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/shelters")]
public class SheltersController : ControllerBase
{
    private readonly IShelterService _shelterService;
    private readonly ILogger<SheltersController> _logger;

    public SheltersController(IShelterService shelterService, ILogger<SheltersController> logger)
    {
        _shelterService = shelterService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active shelters, or paged if page parameter is passed.
    /// Preserves compatibility with existing callers expecting ShelterDto[].
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ShelterQueryParameters parameters, CancellationToken cancellationToken)
    {
        if (parameters.Page.HasValue && parameters.Page.Value > 0)
        {
            var paged = await _shelterService.GetSheltersPagedAsync(parameters, cancellationToken);
            return Ok(paged);
        }

        var shelters = await _shelterService.GetAllSheltersAsync(cancellationToken);
        return Ok(shelters);
    }

    /// <summary>
    /// Explicit paged endpoint for shelter management table.
    /// </summary>
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResultDto<ShelterDto>>> GetPaged(
        [FromQuery] ShelterQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _shelterService.GetSheltersPagedAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets all shelters without pagination (useful for exports and lookups).
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<ShelterDto>>> GetAll(CancellationToken cancellationToken)
    {
        var shelters = await _shelterService.GetAllSheltersAsync(cancellationToken);
        return Ok(shelters);
    }

    /// <summary>
    /// Gets summary counts for shelter management KPI cards.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ShelterSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _shelterService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets a single shelter by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ShelterDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var shelter = await _shelterService.GetShelterByIdAsync(id, cancellationToken);
        if (shelter == null)
        {
            return NotFound(new { message = $"Shelter with ID '{id}' was not found." });
        }
        return Ok(shelter);
    }

    /// <summary>
    /// Registers a new shelter.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ShelterDto>> Create([FromBody] CreateShelterDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _shelterService.CreateShelterAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering shelter");
            return StatusCode(500, new { message = "An error occurred while registering the shelter." });
        }
    }
}
