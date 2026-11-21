using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/volunteer/directory")]
[Route("api/volunteer-directory")]
public class VolunteerDirectoryController : ControllerBase
{
    private readonly IVolunteerDirectoryService _service;
    private readonly ILogger<VolunteerDirectoryController> _logger;

    public VolunteerDirectoryController(
        IVolunteerDirectoryService service,
        ILogger<VolunteerDirectoryController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<VolunteerDirectoryOverviewDto>>> GetDirectory(
        [FromQuery] VolunteerDirectoryQueryDto query)
    {
        try
        {
            var overview = await _service.GetOverviewAsync(query);
            return Ok(ApiResponse<VolunteerDirectoryOverviewDto>.Ok(overview, "Volunteer directory retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get volunteer directory");
            return StatusCode(500, ApiResponse<VolunteerDirectoryOverviewDto>.Fail("Error retrieving volunteer directory."));
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<VolunteerDirectoryStatsDto>>> GetStats()
    {
        try
        {
            var stats = await _service.GetStatsAsync();
            return Ok(ApiResponse<VolunteerDirectoryStatsDto>.Ok(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get volunteer directory stats");
            return StatusCode(500, ApiResponse<VolunteerDirectoryStatsDto>.Fail("Error retrieving directory statistics."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<VolunteerDirectoryItemDto>>> GetById(string id)
    {
        try
        {
            var volunteer = await _service.GetByIdAsync(id);
            if (volunteer == null)
            {
                return NotFound(ApiResponse<VolunteerDirectoryItemDto>.Fail($"Volunteer '{id}' not found."));
            }
            return Ok(ApiResponse<VolunteerDirectoryItemDto>.Ok(volunteer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get volunteer {Id}", id);
            return StatusCode(500, ApiResponse<VolunteerDirectoryItemDto>.Fail("Error retrieving volunteer profile."));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VolunteerDirectoryItemDto>>> Create(
        [FromBody] CreateVolunteerDirectoryRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Location))
            {
                return BadRequest(ApiResponse<VolunteerDirectoryItemDto>.Fail("Volunteer name and location are required."));
            }

            var created = await _service.CreateAsync(request);
            return Ok(ApiResponse<VolunteerDirectoryItemDto>.Ok(created, "Volunteer registered successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register volunteer");
            return StatusCode(500, ApiResponse<VolunteerDirectoryItemDto>.Fail("Error registering volunteer."));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<VolunteerDirectoryItemDto>>> Update(
        string id,
        [FromBody] UpdateVolunteerDirectoryRequest request)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, request);
            if (updated == null)
            {
                return NotFound(ApiResponse<VolunteerDirectoryItemDto>.Fail($"Volunteer '{id}' not found."));
            }
            return Ok(ApiResponse<VolunteerDirectoryItemDto>.Ok(updated, "Volunteer profile updated successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update volunteer {Id}", id);
            return StatusCode(500, ApiResponse<VolunteerDirectoryItemDto>.Fail("Error updating volunteer profile."));
        }
    }

    [HttpPatch("{id}/verification")]
    public async Task<ActionResult<ApiResponse<VolunteerDirectoryItemDto>>> UpdateVerification(
        string id,
        [FromBody] UpdateVolunteerVerificationRequest request)
    {
        try
        {
            var updated = await _service.UpdateVerificationAsync(id, request.Verification);
            if (updated == null)
            {
                return NotFound(ApiResponse<VolunteerDirectoryItemDto>.Fail($"Volunteer '{id}' not found."));
            }
            return Ok(ApiResponse<VolunteerDirectoryItemDto>.Ok(updated, "Verification status updated."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update verification for volunteer {Id}", id);
            return StatusCode(500, ApiResponse<VolunteerDirectoryItemDto>.Fail("Error updating verification status."));
        }
    }

    [HttpPatch("{id}/availability")]
    public async Task<ActionResult<ApiResponse<VolunteerDirectoryItemDto>>> UpdateAvailability(
        string id,
        [FromBody] UpdateVolunteerAvailabilityRequest request)
    {
        try
        {
            var updated = await _service.UpdateAvailabilityAsync(id, request.Availability);
            if (updated == null)
            {
                return NotFound(ApiResponse<VolunteerDirectoryItemDto>.Fail($"Volunteer '{id}' not found."));
            }
            return Ok(ApiResponse<VolunteerDirectoryItemDto>.Ok(updated, "Availability status updated."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update availability for volunteer {Id}", id);
            return StatusCode(500, ApiResponse<VolunteerDirectoryItemDto>.Fail("Error updating availability status."));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string id)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<bool>.Fail($"Volunteer '{id}' not found."));
            }
            return Ok(ApiResponse<bool>.Ok(true, $"Volunteer '{id}' removed successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete volunteer {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Error deleting volunteer."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportCsv([FromQuery] VolunteerDirectoryQueryDto query)
    {
        try
        {
            var bytes = await _service.ExportCsvAsync(query);
            var fileName = $"volunteer-directory-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export volunteer directory CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting volunteer directory."));
        }
    }
}
