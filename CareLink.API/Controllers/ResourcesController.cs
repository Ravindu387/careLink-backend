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
    /// Retrieves all resource items with optional filtering by search query, location area, stock status, or availability tier.
    /// Supports pagination when page parameter is provided.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? search,
        [FromQuery] string? area,
        [FromQuery] string? status,
        [FromQuery] string? availability,
        [FromQuery] string? category,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var parameters = new ResourceQueryParameters
        {
            Search = search,
            Area = area,
            Status = status,
            Availability = availability,
            Category = category,
            Page = page.GetValueOrDefault(1),
            PageSize = pageSize.GetValueOrDefault(10)
        };

        if (page.HasValue && page.Value > 0)
        {
            var pagedResult = await _resourceService.GetPagedResourcesAsync(parameters, cancellationToken);
            return Ok(pagedResult);
        }

        var items = await _resourceService.GetAllAsync(parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Explicit paged endpoint for inventory resources with filters.
    /// </summary>
    [HttpGet("paged")]
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
    /// Retrieves operational stock metrics and critical shortage indicators across all warehouses.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(ResourceMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResourceMetricsDto>> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _resourceService.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
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
        ResourceDto? item = null;
        if (int.TryParse(id, out var intId))
        {
            item = await _resourceService.GetByIdAsync(intId, cancellationToken);
        }

        item ??= await _resourceService.GetResourceByIdAsync(id, cancellationToken);

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
            _logger.LogInformation("Resource '{Name}' created with ID {Id} for shelter {Shelter}", created.Name, created.Id, created.Shelter);
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
    /// Updates stock counts and status for an inventory item.
    /// </summary>
    [HttpPatch("{id:int}/stock")]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceDto>> UpdateStock(
        int id,
        [FromBody] UpdateResourceStockDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _resourceService.UpdateStockAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Resource item '{id}' not found." });
        }
        return Ok(updated);
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
    /// Deletes a resource line item.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken)
    {
        bool deleted;
        if (int.TryParse(id, out var intId))
        {
            deleted = await _resourceService.DeleteAsync(intId, cancellationToken);
        }
        else
        {
            deleted = await _resourceService.DeleteResourceAsync(id, cancellationToken);
        }

        if (!deleted)
        {
            return NotFound(new { message = $"Resource item '{id}' not found." });
        }
        return Ok(new { id, message = $"Resource item '{id}' deleted successfully." });
    }

    /// <summary>
    /// Retrieves all emergency resource allocation requests.
    /// </summary>
    [HttpGet("requests")]
    [ProducesResponseType(typeof(IReadOnlyList<ResourceRequestResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ResourceRequestResponseDto>>> GetRequests(CancellationToken cancellationToken)
    {
        var requests = await _resourceService.GetRequestsAsync(cancellationToken);
        return Ok(requests);
    }

    /// <summary>
    /// Submits a new emergency resource request to warehouses.
    /// </summary>
    [HttpPost("requests")]
    [ProducesResponseType(typeof(ResourceRequestResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResourceRequestResponseDto>> CreateRequest(
        [FromBody] CreateResourceRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var request = await _resourceService.CreateRequestAsync(dto, cancellationToken);
        _logger.LogInformation("Resource request {Id} submitted for {Quantity}x {Name}", request.Id, request.Quantity, request.ResourceName);
        return CreatedAtAction(nameof(GetRequests), new { id = request.Id }, request);
    }

    /// <summary>
    /// Resets all resources and resource requests to baseline seed fixtures.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetDefaultData(CancellationToken cancellationToken)
    {
        await _resourceService.ResetDefaultDataAsync(cancellationToken);
        _logger.LogInformation("Resource inventory reset to default fixtures.");
        return Ok(new { message = "Resource inventory reset to default fixtures." });
    }
}
