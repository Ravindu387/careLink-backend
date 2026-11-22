using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/campaigns")]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campaignService;
    private readonly ILogger<CampaignsController> _logger;

    public CampaignsController(ICampaignService campaignService, ILogger<CampaignsController> logger)
    {
        _campaignService = campaignService;
        _logger = logger;
    }

    /// <summary>
    /// Gets campaigns with optional search, filtering, and pagination.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<CampaignDto>>> Get(
        [FromQuery] CampaignQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _campaignService.GetPagedCampaignsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets all campaigns without pagination.
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<CampaignDto>>> GetAll(CancellationToken cancellationToken)
    {
        var campaigns = await _campaignService.GetAllCampaignsAsync(cancellationToken);
        return Ok(campaigns);
    }

    /// <summary>
    /// Gets KPI summary metrics for campaign dashboard.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<CampaignSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _campaignService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets a single campaign by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CampaignDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var campaign = await _campaignService.GetCampaignByIdAsync(id, cancellationToken);
        if (campaign == null)
        {
            return NotFound(new { message = $"Campaign with ID '{id}' was not found." });
        }
        return Ok(campaign);
    }

    /// <summary>
    /// Creates a new fundraising campaign.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CampaignDto>> Create(
        [FromBody] CreateCampaignDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _campaignService.CreateCampaignAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign");
            return StatusCode(500, new { message = "An error occurred while creating the campaign." });
        }
    }

    /// <summary>
    /// Updates an existing campaign.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CampaignDto>> Update(
        string id,
        [FromBody] UpdateCampaignDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _campaignService.UpdateCampaignAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Campaign with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Updates the status of a campaign (e.g. Active, Completed, Archived).
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<CampaignDto>> UpdateStatus(
        string id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest(new { message = "Status is required." });
        }

        var updated = await _campaignService.UpdateStatusAsync(id, request.Status, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Campaign with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Archives a campaign.
    /// </summary>
    [HttpPost("{id}/archive")]
    public async Task<ActionResult<CampaignDto>> Archive(string id, CancellationToken cancellationToken)
    {
        var updated = await _campaignService.UpdateStatusAsync(id, "Archived", cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Campaign with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Restores an archived campaign back to Active.
    /// </summary>
    [HttpPost("{id}/restore")]
    public async Task<ActionResult<CampaignDto>> Restore(string id, CancellationToken cancellationToken)
    {
        var updated = await _campaignService.UpdateStatusAsync(id, "Active", cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Campaign with ID '{id}' was not found." });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a campaign.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var deleted = await _campaignService.DeleteCampaignAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = $"Campaign with ID '{id}' was not found." });
        }
        return NoContent();
    }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
