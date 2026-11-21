using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/ai-volunteer-matching")]
public class AIVolunteerMatchingController : ControllerBase
{
    private readonly IAIVolunteerMatchingService _matchingService;
    private readonly ILogger<AIVolunteerMatchingController> _logger;

    public AIVolunteerMatchingController(
        IAIVolunteerMatchingService matchingService,
        ILogger<AIVolunteerMatchingController> logger)
    {
        _matchingService = matchingService;
        _logger = logger;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<MatchingSummaryDto>>> GetSummary()
    {
        try
        {
            var summary = await _matchingService.GetSummaryAsync();
            return Ok(ApiResponse<MatchingSummaryDto>.Ok(summary, "AI matching summary retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get AI matching summary");
            return StatusCode(500, ApiResponse<MatchingSummaryDto>.Fail("Error retrieving matching summary."));
        }
    }

    [HttpGet("tasks")]
    public async Task<ActionResult<ApiResponse<List<MatchingTask>>>> GetTasks()
    {
        try
        {
            var tasks = await _matchingService.GetTasksAsync();
            return Ok(ApiResponse<List<MatchingTask>>.Ok(tasks, "Open matching tasks retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get matching tasks");
            return StatusCode(500, ApiResponse<List<MatchingTask>>.Fail("Error retrieving open tasks."));
        }
    }

    [HttpGet("tasks/{id}")]
    public async Task<ActionResult<ApiResponse<MatchingTask>>> GetTaskById(string id)
    {
        try
        {
            var task = await _matchingService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound(ApiResponse<MatchingTask>.Fail($"Task {id} not found."));
            }
            return Ok(ApiResponse<MatchingTask>.Ok(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get task {Id}", id);
            return StatusCode(500, ApiResponse<MatchingTask>.Fail("Error retrieving task."));
        }
    }

    [HttpGet("candidates")]
    public async Task<ActionResult<ApiResponse<List<MatchingCandidate>>>> GetCandidates(
        [FromQuery] string taskId,
        [FromQuery] string? search = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                return BadRequest(ApiResponse<List<MatchingCandidate>>.Fail("taskId is required to retrieve recommended candidates."));
            }

            var candidates = await _matchingService.GetCandidatesForTaskAsync(taskId, search);
            return Ok(ApiResponse<List<MatchingCandidate>>.Ok(candidates, $"Retrieved {candidates.Count} candidate recommendations."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get candidates for task {TaskId}", taskId);
            return StatusCode(500, ApiResponse<List<MatchingCandidate>>.Fail("Error retrieving candidate recommendations."));
        }
    }

    [HttpPost("decisions")]
    public async Task<ActionResult<ApiResponse<MatchingDecisionRecord>>> RecordDecision(
        [FromBody] RecordDecisionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.TaskId) || string.IsNullOrWhiteSpace(request.CandidateId))
            {
                return BadRequest(ApiResponse<MatchingDecisionRecord>.Fail("TaskId and CandidateId are required."));
            }

            if (string.IsNullOrWhiteSpace(request.Rationale))
            {
                return BadRequest(ApiResponse<MatchingDecisionRecord>.Fail("Decision rationale is required."));
            }

            var record = await _matchingService.RecordDecisionAsync(request);
            return Ok(ApiResponse<MatchingDecisionRecord>.Ok(record, $"Match decision '{record.Decision}' recorded successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record decision for candidate {CandidateId} on task {TaskId}", request.CandidateId, request.TaskId);
            return StatusCode(500, ApiResponse<MatchingDecisionRecord>.Fail("Error recording matching decision."));
        }
    }

    [HttpGet("decisions")]
    public async Task<ActionResult<ApiResponse<List<MatchingDecisionRecord>>>> GetDecisionHistory()
    {
        try
        {
            var history = await _matchingService.GetDecisionHistoryAsync();
            return Ok(ApiResponse<List<MatchingDecisionRecord>>.Ok(history, "Decision history retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get decision history");
            return StatusCode(500, ApiResponse<List<MatchingDecisionRecord>>.Fail("Error retrieving decision history."));
        }
    }

    [HttpDelete("decisions/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDecision(string id)
    {
        try
        {
            var deleted = await _matchingService.DeleteDecisionAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<bool>.Fail($"Decision {id} not found."));
            }
            return Ok(ApiResponse<bool>.Ok(true, "Decision record deleted successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete decision {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Error deleting decision record."));
        }
    }
}
