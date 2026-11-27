using Microsoft.AspNetCore.Authorization;
using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/volunteer-verifications")]
[Route("api/volunteer/verification")]
[Route("api/volunteer/verifications")]
public class VolunteerVerificationsController : ControllerBase
{
    private readonly IVolunteerVerificationsService _service;
    private readonly ILogger<VolunteerVerificationsController> _logger;

    public VolunteerVerificationsController(
        IVolunteerVerificationsService service,
        ILogger<VolunteerVerificationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<VolunteerVerificationStatsDto>>> GetStats()
    {
        try
        {
            var stats = await _service.GetStatsAsync();
            return Ok(ApiResponse<VolunteerVerificationStatsDto>.Ok(stats, "Verification statistics retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get verification stats");
            return StatusCode(500, ApiResponse<VolunteerVerificationStatsDto>.Fail("Error retrieving verification statistics."));
        }
    }

    [HttpGet("pending")]
    public async Task<ActionResult<ApiResponse<List<VolunteerVerificationApplication>>>> GetPending()
    {
        try
        {
            var pending = await _service.GetPendingApplicationsAsync();
            return Ok(ApiResponse<List<VolunteerVerificationApplication>>.Ok(pending, "Pending verification applications retrieved."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pending applications");
            return StatusCode(500, ApiResponse<List<VolunteerVerificationApplication>>.Fail("Error retrieving pending applications."));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<VolunteerVerificationApplication>>>> GetApplications(
        [FromQuery] VolunteerVerificationQueryDto query)
    {
        try
        {
            var paged = await _service.GetApplicationsAsync(query);
            return Ok(ApiResponse<PaginatedResult<VolunteerVerificationApplication>>.Ok(paged));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get verification applications");
            return StatusCode(500, ApiResponse<PaginatedResult<VolunteerVerificationApplication>>.Fail("Error retrieving verification applications."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<VolunteerVerificationApplication>>> GetById(string id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<VolunteerVerificationApplication>.Fail($"Verification application {id} not found."));
            }
            return Ok(ApiResponse<VolunteerVerificationApplication>.Ok(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get application {Id}", id);
            return StatusCode(500, ApiResponse<VolunteerVerificationApplication>.Fail("Error retrieving application."));
        }
    }

    [HttpPut("{id}/approve")]
    [HttpPatch("{id}/approve")]
    public async Task<ActionResult<ApiResponse<VolunteerVerificationApplication>>> Approve(
        string id,
        [FromBody] ReviewVerificationRequest? request)
    {
        try
        {
            var approved = await _service.ApproveAsync(id, request);
            if (approved == null)
            {
                return NotFound(ApiResponse<VolunteerVerificationApplication>.Fail($"Verification application {id} not found."));
            }
            return Ok(ApiResponse<VolunteerVerificationApplication>.Ok(approved, $"Application {id} approved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve application {Id}", id);
            return StatusCode(500, ApiResponse<VolunteerVerificationApplication>.Fail("Error approving application."));
        }
    }

    [HttpPut("{id}/reject")]
    [HttpPatch("{id}/reject")]
    public async Task<ActionResult<ApiResponse<VolunteerVerificationApplication>>> Reject(
        string id,
        [FromBody] ReviewVerificationRequest? request)
    {
        try
        {
            var rejected = await _service.RejectAsync(id, request);
            if (rejected == null)
            {
                return NotFound(ApiResponse<VolunteerVerificationApplication>.Fail($"Verification application {id} not found."));
            }
            return Ok(ApiResponse<VolunteerVerificationApplication>.Ok(rejected, $"Application {id} rejected."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject application {Id}", id);
            return StatusCode(500, ApiResponse<VolunteerVerificationApplication>.Fail("Error rejecting application."));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VolunteerVerificationApplication>>> Create(
        [FromBody] CreateVerificationApplicationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Applicant))
            {
                return BadRequest(ApiResponse<VolunteerVerificationApplication>.Fail("Applicant name is required."));
            }

            var created = await _service.CreateAsync(request);
            return Ok(ApiResponse<VolunteerVerificationApplication>.Ok(created, "Verification application created successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create verification application");
            return StatusCode(500, ApiResponse<VolunteerVerificationApplication>.Fail("Error creating verification application."));
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
                return NotFound(ApiResponse<bool>.Fail($"Verification application {id} not found."));
            }
            return Ok(ApiResponse<bool>.Ok(true, $"Application {id} deleted successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete application {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Error deleting application."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] VolunteerVerificationQueryDto query)
    {
        try
        {
            var bytes = await _service.ExportCsvAsync(query);
            var fileName = $"carelink-volunteer-verifications-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export verification applications CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting CSV."));
        }
    }
}
