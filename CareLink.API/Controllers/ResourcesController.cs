using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/resources")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _service;
    private readonly ILogger<ResourcesController> _logger;

    public ResourcesController(
        IResourceService service,
        ILogger<ResourcesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all resource items with optional filtering by search query, location area, stock status, or availability tier.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ResourceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ResourceDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? area,
        [FromQuery] string? status,
        [FromQuery] string? availability,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var query = new ResourceQueryParameters
        {
            Search = search,
            Area = area,
            Status = status,
            Availability = availability,
            Category = category
        };

        var items = await _service.GetAllAsync(query, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Retrieves operational stock metrics and critical shortage indicators across all warehouses.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(ResourceMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResourceMetricsDto>> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves a single resource inventory item by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { message = $"Resource item '{id}' not found." });
        }
        return Ok(item);
    }

    /// <summary>
    /// Creates a new resource inventory line item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResourceDto>> Create(
        [FromBody] CreateResourceItemDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateItemAsync(dto, cancellationToken);
        _logger.LogInformation("Resource '{Name}' created with ID {Id} at {Location}", created.Name, created.Id, created.Location);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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
        var updated = await _service.UpdateStockAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Resource item '{id}' not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a resource line item.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
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
        var requests = await _service.GetRequestsAsync(cancellationToken);
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

        var request = await _service.CreateRequestAsync(dto, cancellationToken);
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
        await _service.ResetDefaultDataAsync(cancellationToken);
        _logger.LogInformation("Resource inventory reset to default fixtures.");
        return Ok(new { message = "Resource inventory reset to default fixtures." });
    }
}
