using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/donations")]
public class DonationsController : ControllerBase
{
    private readonly IDonationService _donationService;
    private readonly ILogger<DonationsController> _logger;

    public DonationsController(IDonationService donationService, ILogger<DonationsController> logger)
    {
        _donationService = donationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets donation records with optional search, filtering, and pagination.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<DonationDto>>> Get(
        [FromQuery] DonationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _donationService.GetPagedDonationsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets all donation records without pagination.
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<DonationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var donations = await _donationService.GetAllDonationsAsync(cancellationToken);
        return Ok(donations);
    }

    /// <summary>
    /// Gets summary counts and metrics for donation tabs and dashboard.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DonationSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _donationService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets a single donation record by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DonationDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var donation = await _donationService.GetDonationByIdAsync(id, cancellationToken);
        if (donation == null)
        {
            return NotFound(new { message = $"Donation with ID '{id}' was not found." });
        }
        return Ok(donation);
    }

    /// <summary>
    /// Records a new monetary or material donation.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DonationDto>> Create(
        [FromBody] CreateDonationDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _donationService.CreateDonationAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing donation record.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<DonationDto>> Update(
        string id,
        [FromBody] UpdateDonationDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _donationService.UpdateDonationAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Donation with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Updates the verification status of a donation (e.g. Verified, Pending, Flagged).
    /// </summary>
    [HttpPatch("{id}/verification")]
    public async Task<ActionResult<DonationDto>> UpdateVerification(
        string id,
        [FromBody] UpdateDonationStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Verification))
        {
            return BadRequest(new { message = "Verification status is required." });
        }

        var updated = await _donationService.UpdateDonationVerificationAsync(id, dto.Verification, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Donation with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Updates the allocation status of a donation (e.g. Allocated, Partially Allocated, Unallocated, On Hold).
    /// </summary>
    [HttpPatch("{id}/allocation")]
    public async Task<ActionResult<DonationDto>> UpdateAllocation(
        string id,
        [FromBody] UpdateDonationStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Allocation))
        {
            return BadRequest(new { message = "Allocation status is required." });
        }

        var updated = await _donationService.UpdateDonationAllocationAsync(id, dto.Allocation, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Donation with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Performs bulk actions on multiple donations (e.g. Verify, Allocate, Flag, Hold).
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult> BulkAction(
        [FromBody] BulkDonationActionDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || dto.Ids == null || !dto.Ids.Any())
        {
            return BadRequest(new { message = "At least one donation ID and a valid action are required." });
        }

        var affectedCount = await _donationService.BulkActionAsync(dto, cancellationToken);
        return Ok(new
        {
            message = $"{dto.Action} completed for {affectedCount} donation(s).",
            affectedCount
        });
    }

    /// <summary>
    /// Deletes a donation record.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var deleted = await _donationService.DeleteDonationAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = $"Donation with ID '{id}' was not found." });
        }
        return NoContent();
    }
}
