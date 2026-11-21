using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/assignments")]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentsService _assignmentsService;
    private readonly ILogger<AssignmentsController> _logger;

    public AssignmentsController(IAssignmentsService assignmentsService, ILogger<AssignmentsController> logger)
    {
        _assignmentsService = assignmentsService;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<AssignmentsStatsDto>>> GetStats()
    {
        try
        {
            var stats = await _assignmentsService.GetStatsAsync();
            return Ok(ApiResponse<AssignmentsStatsDto>.Ok(stats, "Assignments statistics retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get assignments stats");
            return StatusCode(500, ApiResponse<AssignmentsStatsDto>.Fail("Error retrieving assignments stats."));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<AssignmentItem>>>> GetAssignments([FromQuery] AssignmentsQueryDto query)
    {
        try
        {
            var paged = await _assignmentsService.GetAssignmentsAsync(query);
            return Ok(ApiResponse<PaginatedResult<AssignmentItem>>.Ok(paged));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get assignments");
            return StatusCode(500, ApiResponse<PaginatedResult<AssignmentItem>>.Fail("Error retrieving assignments."));
        }
    }

    [HttpGet("options")]
    public async Task<ActionResult<ApiResponse<AssignmentOptionsDto>>> GetOptions()
    {
        try
        {
            var options = await _assignmentsService.GetOptionsAsync();
            return Ok(ApiResponse<AssignmentOptionsDto>.Ok(options));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get assignment options");
            return StatusCode(500, ApiResponse<AssignmentOptionsDto>.Fail("Error retrieving assignment options."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AssignmentItem>>> GetById(string id)
    {
        try
        {
            var item = await _assignmentsService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<AssignmentItem>.Fail($"Assignment {id} not found."));
            }
            return Ok(ApiResponse<AssignmentItem>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get assignment {Id}", id);
            return StatusCode(500, ApiResponse<AssignmentItem>.Fail("Error retrieving assignment."));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AssignmentItem>>> CreateAssignment([FromBody] CreateAssignmentRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Volunteer) || string.IsNullOrWhiteSpace(request.Task))
            {
                return BadRequest(ApiResponse<AssignmentItem>.Fail("Volunteer name and Task are required."));
            }

            var created = await _assignmentsService.CreateAssignmentAsync(request);
            return Ok(ApiResponse<AssignmentItem>.Ok(created, "Assignment created successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create assignment");
            return StatusCode(500, ApiResponse<AssignmentItem>.Fail("Error creating assignment."));
        }
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponse<AssignmentItem>>> UpdateAssignment(string id, [FromBody] UpdateAssignmentRequest request)
    {
        try
        {
            var updated = await _assignmentsService.UpdateAssignmentAsync(id, request);
            if (updated == null)
            {
                return NotFound(ApiResponse<AssignmentItem>.Fail($"Assignment {id} not found."));
            }
            return Ok(ApiResponse<AssignmentItem>.Ok(updated, "Assignment updated successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update assignment {Id}", id);
            return StatusCode(500, ApiResponse<AssignmentItem>.Fail("Error updating assignment."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] AssignmentsQueryDto query)
    {
        try
        {
            var bytes = await _assignmentsService.ExportCsvAsync(query);
            var fileName = $"carelink-assignments-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export assignments CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting assignments."));
        }
    }
}
