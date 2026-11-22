using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/documents")]
[Route("api/partner/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IPartnerDocumentService _service;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IPartnerDocumentService service,
        ILogger<DocumentsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves controlled partner documents with optional filtering by search text, tab, type, owner, visibility, and status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PartnerDocumentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? tab,
        [FromQuery] string? type,
        [FromQuery] string? owner,
        [FromQuery] string? visibility,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = new PartnerDocumentQueryParameters
        {
            Search = search,
            Tab = tab,
            Type = type,
            Owner = owner,
            Visibility = visibility,
            Status = status
        };

        var results = await _service.GetAllAsync(query, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Retrieves aggregate metrics for partner documents including active records, review counts, and verified assurance.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(PartnerDocumentMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves a single document by its identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PartnerDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { error = $"Document '{id}' was not found." });
        }

        return Ok(item);
    }

    /// <summary>
    /// Uploads and registers a new controlled partner document.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PartnerDocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePartnerDocumentDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Document {Id} uploaded by {Owner}: {Name}", result.Id, result.Owner, result.Name);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates document metadata and properties.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PartnerDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdatePartnerDocumentDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Document '{id}' was not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Updates status of a document (e.g. Published, Approved, Under Review, Archived).
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(PartnerDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(
        string id,
        [FromBody] UpdatePartnerDocumentStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateStatusAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Document '{id}' was not found." });
        }

        _logger.LogInformation("Document {Id} status updated to {Status}", id, dto.Status);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a document record.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var success = await _service.DeleteAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(new { error = $"Document '{id}' was not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Resets documents to demonstration baseline dataset.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _service.ResetToDefaultAsync(cancellationToken);
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(new { message = "Documents reset to default state.", metrics });
    }
}
