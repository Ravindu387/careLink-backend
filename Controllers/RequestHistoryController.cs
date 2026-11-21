using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/request-history")]
public class RequestHistoryController : ControllerBase
{
    private readonly IRequestHistoryService _requestHistoryService;
    private readonly ILogger<RequestHistoryController> _logger;

    public RequestHistoryController(IRequestHistoryService requestHistoryService, ILogger<RequestHistoryController> logger)
    {
        _requestHistoryService = requestHistoryService;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<RequestHistoryStatsDto>>> GetStats()
    {
        try
        {
            var stats = await _requestHistoryService.GetStatsAsync();
            return Ok(ApiResponse<RequestHistoryStatsDto>.Ok(stats, "Request history statistics retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get history stats");
            return StatusCode(500, ApiResponse<RequestHistoryStatsDto>.Fail("Error retrieving history stats."));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<HistoryEventItem>>>> GetEvents([FromQuery] RequestHistoryQueryDto query)
    {
        try
        {
            var paged = await _requestHistoryService.GetEventsAsync(query);
            return Ok(ApiResponse<PaginatedResult<HistoryEventItem>>.Ok(paged));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get history events");
            return StatusCode(500, ApiResponse<PaginatedResult<HistoryEventItem>>.Fail("Error retrieving history events."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<HistoryEventItem>>> GetById(string id)
    {
        try
        {
            var item = await _requestHistoryService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<HistoryEventItem>.Fail($"History event {id} not found."));
            }
            return Ok(ApiResponse<HistoryEventItem>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get history event {Id}", id);
            return StatusCode(500, ApiResponse<HistoryEventItem>.Fail("Error retrieving history event."));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<HistoryEventItem>>> LogEvent([FromBody] LogHistoryEventRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.CaseId) || string.IsNullOrWhiteSpace(request.Action))
            {
                return BadRequest(ApiResponse<HistoryEventItem>.Fail("Case ID and Action are required."));
            }

            var created = await _requestHistoryService.LogEventAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<HistoryEventItem>.Ok(created, "Event logged successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log event");
            return StatusCode(500, ApiResponse<HistoryEventItem>.Fail("Error logging history event."));
        }
    }

    [HttpGet("actors")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetActors()
    {
        try
        {
            var actors = await _requestHistoryService.GetUniqueActorsAsync();
            return Ok(ApiResponse<List<string>>.Ok(actors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get actors");
            return StatusCode(500, ApiResponse<List<string>>.Fail("Error retrieving actors."));
        }
    }

    [HttpGet("cases")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetCases()
    {
        try
        {
            var cases = await _requestHistoryService.GetUniqueCaseIdsAsync();
            return Ok(ApiResponse<List<string>>.Ok(cases));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cases");
            return StatusCode(500, ApiResponse<List<string>>.Fail("Error retrieving case IDs."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] RequestHistoryQueryDto query)
    {
        try
        {
            var bytes = await _requestHistoryService.ExportCsvAsync(query);
            var fileName = $"carelink-request-history-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export request history CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting request history."));
        }
    }
}
