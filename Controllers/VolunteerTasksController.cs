using Microsoft.AspNetCore.Authorization;
using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/volunteer/tasks")]
[Route("api/volunteer-tasks")]
public class VolunteerTasksController : ControllerBase
{
    private readonly IVolunteerTasksService _service;
    private readonly ILogger<VolunteerTasksController> _logger;

    public VolunteerTasksController(
        IVolunteerTasksService service,
        ILogger<VolunteerTasksController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<VolunteerTasksOverviewDto>>> GetTasks(
        [FromQuery] VolunteerTaskQueryDto query)
    {
        try
        {
            var overview = await _service.GetTasksOverviewAsync(query);
            return Ok(ApiResponse<VolunteerTasksOverviewDto>.Ok(overview, "Volunteer tasks retrieved."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get volunteer tasks");
            return StatusCode(500, ApiResponse<VolunteerTasksOverviewDto>.Fail("Error retrieving tasks."));
        }
    }

    [HttpGet("counts")]
    public async Task<ActionResult<ApiResponse<VolunteerTaskTabsCountDto>>> GetTabCounts()
    {
        try
        {
            var counts = await _service.GetTabCountsAsync();
            return Ok(ApiResponse<VolunteerTaskTabsCountDto>.Ok(counts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get volunteer task counts");
            return StatusCode(500, ApiResponse<VolunteerTaskTabsCountDto>.Fail("Error retrieving task counts."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<VolunteerTaskDto>>> GetTaskById(string id)
    {
        try
        {
            var task = await _service.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound(ApiResponse<VolunteerTaskDto>.Fail($"Task '{id}' not found."));
            }
            return Ok(ApiResponse<VolunteerTaskDto>.Ok(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get task {Id}", id);
            return StatusCode(500, ApiResponse<VolunteerTaskDto>.Fail("Error retrieving task."));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VolunteerTaskDto>>> CreateTask(
        [FromBody] CreateVolunteerTaskRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Location))
            {
                return BadRequest(ApiResponse<VolunteerTaskDto>.Fail("Task title and location are required."));
            }

            var task = await _service.CreateTaskAsync(request);
            return Ok(ApiResponse<VolunteerTaskDto>.Ok(task, "Task created successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create volunteer task");
            return StatusCode(500, ApiResponse<VolunteerTaskDto>.Fail("Error creating task."));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<VolunteerTaskDto>>> UpdateTask(
        string id,
        [FromBody] UpdateVolunteerTaskRequest request)
    {
        try
        {
            var updated = await _service.UpdateTaskAsync(id, request);
            if (updated == null)
            {
                return NotFound(ApiResponse<VolunteerTaskDto>.Fail($"Task '{id}' not found."));
            }
            return Ok(ApiResponse<VolunteerTaskDto>.Ok(updated, "Task updated successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update task {Id}", id);
            return StatusCode(500, ApiResponse<VolunteerTaskDto>.Fail("Error updating task."));
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<VolunteerTaskDto>>> UpdateStatus(
        string id,
        [FromBody] UpdateTaskStatusRequest request)
    {
        try
        {
            var updated = await _service.UpdateTaskStatusAsync(id, request.Status);
            if (updated == null)
            {
                return NotFound(ApiResponse<VolunteerTaskDto>.Fail($"Task '{id}' not found."));
            }
            return Ok(ApiResponse<VolunteerTaskDto>.Ok(updated, "Task status updated."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update status for task {Id}", id);
            return StatusCode(500, ApiResponse<VolunteerTaskDto>.Fail("Error updating status."));
        }
    }

    [HttpPost("{id}/assign")]
    public async Task<ActionResult<ApiResponse<VolunteerTaskDto>>> AdjustVolunteerAssignment(
        string id,
        [FromBody] AssignVolunteerRequest request)
    {
        try
        {
            var updated = await _service.AdjustAssignedVolunteersAsync(id, request.Delta);
            if (updated == null)
            {
                return NotFound(ApiResponse<VolunteerTaskDto>.Fail($"Task '{id}' not found."));
            }
            return Ok(ApiResponse<VolunteerTaskDto>.Ok(updated, "Volunteer assignment updated."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to adjust volunteer assignment for task {Id}", id);
            return StatusCode(500, ApiResponse<VolunteerTaskDto>.Fail("Error adjusting volunteer assignment."));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTask(string id)
    {
        try
        {
            var deleted = await _service.DeleteTaskAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<bool>.Fail($"Task '{id}' not found."));
            }
            return Ok(ApiResponse<bool>.Ok(true, $"Task '{id}' deleted successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete task {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Error deleting task."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportTasksCsv([FromQuery] VolunteerTaskQueryDto query)
    {
        try
        {
            var bytes = await _service.ExportTasksCsvAsync(query);
            var fileName = $"carelink-volunteer-tasks-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export tasks CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting tasks CSV."));
        }
    }
}
