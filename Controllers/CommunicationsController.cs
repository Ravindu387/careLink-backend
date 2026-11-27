using Microsoft.AspNetCore.Authorization;
using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/communications")]
public class CommunicationsController : ControllerBase
{
    private readonly ICommunicationsService _communicationsService;
    private readonly ILogger<CommunicationsController> _logger;

    public CommunicationsController(ICommunicationsService communicationsService, ILogger<CommunicationsController> logger)
    {
        _communicationsService = communicationsService;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<CommunicationsOverviewDto>>> GetOverview()
    {
        try
        {
            var overview = await _communicationsService.GetOverviewAsync();
            return Ok(ApiResponse<CommunicationsOverviewDto>.Ok(overview, "Communications overview retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get communications overview");
            return StatusCode(500, ApiResponse<CommunicationsOverviewDto>.Fail("Error retrieving communications overview."));
        }
    }

    [HttpGet("audiences")]
    public async Task<ActionResult<ApiResponse<List<AudienceOption>>>> GetAudiences()
    {
        try
        {
            var audiences = await _communicationsService.GetAudiencesAsync();
            return Ok(ApiResponse<List<AudienceOption>>.Ok(audiences, "Audiences retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audiences");
            return StatusCode(500, ApiResponse<List<AudienceOption>>.Fail("Error retrieving audiences."));
        }
    }

    [HttpGet("channel-health")]
    public async Task<ActionResult<ApiResponse<List<ChannelHealth>>>> GetChannelHealth()
    {
        try
        {
            var health = await _communicationsService.GetChannelHealthAsync();
            return Ok(ApiResponse<List<ChannelHealth>>.Ok(health, "Channel health retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get channel health");
            return StatusCode(500, ApiResponse<List<ChannelHealth>>.Fail("Error retrieving channel health."));
        }
    }

    [HttpGet("broadcasts")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<Broadcast>>>> GetBroadcasts([FromQuery] BroadcastQueryDto query)
    {
        try
        {
            var result = await _communicationsService.GetBroadcastsAsync(query);
            return Ok(ApiResponse<PaginatedResult<Broadcast>>.Ok(result, "Broadcasts retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get broadcasts");
            return StatusCode(500, ApiResponse<PaginatedResult<Broadcast>>.Fail("Error retrieving broadcasts."));
        }
    }

    [HttpGet("broadcasts/{id}")]
    public async Task<ActionResult<ApiResponse<Broadcast>>> GetBroadcastById(string id)
    {
        try
        {
            var item = await _communicationsService.GetBroadcastByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<Broadcast>.Fail($"Broadcast with ID {id} not found."));
            }
            return Ok(ApiResponse<Broadcast>.Ok(item, "Broadcast retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get broadcast {Id}", id);
            return StatusCode(500, ApiResponse<Broadcast>.Fail("Error retrieving broadcast."));
        }
    }

    [HttpPost("broadcasts")]
    public async Task<ActionResult<ApiResponse<Broadcast>>> CreateBroadcast([FromBody] CreateBroadcastRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(ApiResponse<Broadcast>.Fail("Broadcast title is required."));
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(ApiResponse<Broadcast>.Fail("Broadcast message is required."));
            }

            if (string.IsNullOrWhiteSpace(request.Audience))
            {
                return BadRequest(ApiResponse<Broadcast>.Fail("Target audience is required."));
            }

            if (request.Channels == null || request.Channels.Count == 0)
            {
                return BadRequest(ApiResponse<Broadcast>.Fail("At least one delivery channel must be selected."));
            }

            var created = await _communicationsService.CreateBroadcastAsync(request);
            return Ok(ApiResponse<Broadcast>.Ok(created, "Broadcast sent successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create broadcast");
            return StatusCode(500, ApiResponse<Broadcast>.Fail("Error creating broadcast."));
        }
    }

    [HttpDelete("broadcasts/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteBroadcast(string id)
    {
        try
        {
            var deleted = await _communicationsService.DeleteBroadcastAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<bool>.Fail($"Broadcast {id} not found."));
            }
            return Ok(ApiResponse<bool>.Ok(true, "Broadcast deleted successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete broadcast {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Error deleting broadcast."));
        }
    }

    [HttpPost("broadcasts/{id}/resend")]
    public async Task<ActionResult<ApiResponse<Broadcast>>> ResendBroadcast(string id)
    {
        try
        {
            var item = await _communicationsService.ResendBroadcastAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<Broadcast>.Fail($"Broadcast {id} not found."));
            }
            return Ok(ApiResponse<Broadcast>.Ok(item, "Broadcast resent successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend broadcast {Id}", id);
            return StatusCode(500, ApiResponse<Broadcast>.Fail("Error resending broadcast."));
        }
    }

    [HttpGet("draft")]
    public async Task<ActionResult<ApiResponse<CommunicationDraftDto?>>> GetDraft()
    {
        try
        {
            var draft = await _communicationsService.GetDraftAsync();
            return Ok(ApiResponse<CommunicationDraftDto?>.Ok(draft, "Draft retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get draft");
            return StatusCode(500, ApiResponse<CommunicationDraftDto?>.Fail("Error retrieving draft."));
        }
    }

    [HttpPost("draft")]
    public async Task<ActionResult<ApiResponse<CommunicationDraftDto>>> SaveDraft([FromBody] SaveDraftRequest request)
    {
        try
        {
            var saved = await _communicationsService.SaveDraftAsync(request);
            return Ok(ApiResponse<CommunicationDraftDto>.Ok(saved, "Draft saved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save draft");
            return StatusCode(500, ApiResponse<CommunicationDraftDto>.Fail("Error saving draft."));
        }
    }

    [HttpDelete("draft")]
    public async Task<ActionResult<ApiResponse<bool>>> ClearDraft()
    {
        try
        {
            var cleared = await _communicationsService.ClearDraftAsync();
            return Ok(ApiResponse<bool>.Ok(cleared, "Draft cleared successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear draft");
            return StatusCode(500, ApiResponse<bool>.Fail("Error clearing draft."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] BroadcastQueryDto query)
    {
        try
        {
            var bytes = await _communicationsService.ExportCsvAsync(query);
            var fileName = $"carelink-broadcasts-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export broadcasts CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting broadcasts."));
        }
    }
}
