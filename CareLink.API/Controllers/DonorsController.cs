using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/donors")]
public class DonorsController : ControllerBase
{
    private readonly IDonorService _donorService;
    private readonly ILogger<DonorsController> _logger;

    public DonorsController(IDonorService donorService, ILogger<DonorsController> logger)
    {
        _donorService = donorService;
        _logger = logger;
    }

    /// <summary>
    /// Gets donors with optional search, filtering, and pagination.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<DonorDto>>> Get(
        [FromQuery] DonorQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _donorService.GetPagedDonorsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets all donors without pagination.
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<DonorDto>>> GetAll(CancellationToken cancellationToken)
    {
        var donors = await _donorService.GetAllDonorsAsync(cancellationToken);
        return Ok(donors);
    }

    /// <summary>
    /// Gets KPI summary metrics for donor management.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DonorSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _donorService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets a single donor by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DonorDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var donor = await _donorService.GetDonorByIdAsync(id, cancellationToken);
        if (donor == null)
        {
            return NotFound(new { message = $"Donor with ID '{id}' was not found." });
        }
        return Ok(donor);
    }

    /// <summary>
    /// Registers a new donor profile.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DonorDto>> Create(
        [FromBody] CreateDonorDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _donorService.CreateDonorAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing donor profile.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<DonorDto>> Update(
        string id,
        [FromBody] UpdateDonorDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _donorService.UpdateDonorAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Donor with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Updates donor active/restricted/inactive status.
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<DonorDto>> UpdateStatus(
        string id,
        [FromBody] UpdateDonorStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Status))
        {
            return BadRequest(new { message = "Status is required." });
        }

        var updated = await _donorService.UpdateDonorStatusAsync(id, dto.Status, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Donor with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Updates donor verification status.
    /// </summary>
    [HttpPatch("{id}/verification")]
    public async Task<ActionResult<DonorDto>> UpdateVerification(
        string id,
        [FromBody] UpdateDonorStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Verification))
        {
            return BadRequest(new { message = "Verification status is required." });
        }

        var updated = await _donorService.UpdateDonorVerificationAsync(id, dto.Verification, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Donor with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Performs bulk action on multiple donors (e.g. Activate, Restrict, Deactivate).
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult> BulkAction(
        [FromBody] BulkDonorActionDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || dto.Ids == null || !dto.Ids.Any())
        {
            return BadRequest(new { message = "At least one donor ID and a valid action are required." });
        }

        var affectedCount = await _donorService.BulkActionAsync(dto, cancellationToken);
        return Ok(new
        {
            message = $"{dto.Action} completed for {affectedCount} donor(s).",
            affectedCount
        });
    }

    /// <summary>
    /// Deletes a donor profile.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var deleted = await _donorService.DeleteDonorAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = $"Donor with ID '{id}' was not found." });
        }
        return NoContent();
    }
}
