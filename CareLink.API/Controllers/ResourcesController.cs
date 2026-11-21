using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/resources")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resourceService;
    private readonly ILogger<ResourcesController> _logger;

    public ResourcesController(
        IResourceService resourceService,
        ILogger<ResourcesController> logger)
    {
        _resourceService = resourceService;
        _logger = logger;
    }

    /// <summary>
    /// Gets paged list of inventory resources with filters.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ResourceDto>>> GetPaged(
        [FromQuery] ResourceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _resourceService.GetPagedResourcesAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets all resources without pagination (useful for CSV export).
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<ResourceDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _resourceService.GetAllResourcesAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets summary counts and KPI alert metrics for inventory management.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ResourceSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _resourceService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets a single resource by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var item = await _resourceService.GetResourceByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { message = $"Resource with ID '{id}' was not found." });
        }
        return Ok(item);
    }

    /// <summary>
    /// Adds a new resource to inventory.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResourceDto>> Create(
        [FromBody] CreateResourceDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _resourceService.CreateResourceAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inventory resource");
            return StatusCode(500, new { message = "An error occurred while adding the resource." });
        }
    }

    /// <summary>
    /// Updates details of an existing resource.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ResourceDto>> Update(
        string id,
        [FromBody] UpdateResourceDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _resourceService.UpdateResourceAsync(id, dto, cancellationToken);
            if (updated == null)
            {
                return NotFound(new { message = $"Resource with ID '{id}' was not found." });
            }
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resource {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the resource." });
        }
    }

    /// <summary>
    /// Records a resource distribution to a shelter or recipient.
    /// </summary>
    [HttpPost("distributions")]
    public async Task<ActionResult<ResourceDto>> RecordDistribution(
        [FromBody] RecordDistributionDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _resourceService.RecordDistributionAsync(dto, cancellationToken);
            return Ok(updated);
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
            _logger.LogError(ex, "Error recording distribution for resource {ResourceId}", dto.ResourceId);
            return StatusCode(500, new { message = "An error occurred while recording the distribution." });
        }
    }

    /// <summary>
    /// Gets history of resource distributions.
    /// </summary>
    [HttpGet("distributions")]
    public async Task<ActionResult<IEnumerable<ResourceDistributionDto>>> GetDistributions(
        [FromQuery] string? resourceId,
        CancellationToken cancellationToken)
    {
        var items = await _resourceService.GetDistributionsAsync(resourceId, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Deletes a resource from inventory.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken)
    {
        var deleted = await _resourceService.DeleteResourceAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = $"Resource with ID '{id}' was not found." });
        }
        return NoContent();
    }
}
