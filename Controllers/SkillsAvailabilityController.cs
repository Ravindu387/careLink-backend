using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/volunteer/skills-availability")]
[Route("api/volunteer-skills")]
public class SkillsAvailabilityController : ControllerBase
{
    private readonly ISkillsAvailabilityService _service;
    private readonly ILogger<SkillsAvailabilityController> _logger;

    public SkillsAvailabilityController(
        ISkillsAvailabilityService service,
        ILogger<SkillsAvailabilityController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<SkillsAvailabilityOverviewDto>>> GetOverview(
        [FromQuery] string? area)
    {
        try
        {
            var overview = await _service.GetOverviewAsync(area);
            return Ok(ApiResponse<SkillsAvailabilityOverviewDto>.Ok(overview, "Skills and availability overview retrieved."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get skills and availability overview");
            return StatusCode(500, ApiResponse<SkillsAvailabilityOverviewDto>.Fail("Error retrieving overview."));
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<SkillsAvailabilityStatsDto>>> GetStats()
    {
        try
        {
            var stats = await _service.GetStatsAsync();
            return Ok(ApiResponse<SkillsAvailabilityStatsDto>.Ok(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get availability stats");
            return StatusCode(500, ApiResponse<SkillsAvailabilityStatsDto>.Fail("Error retrieving stats."));
        }
    }

    [HttpGet("skills")]
    public async Task<ActionResult<ApiResponse<List<SkillCoverageItemDto>>>> GetSkills(
        [FromQuery] string? search)
    {
        try
        {
            var skills = await _service.GetSkillsAsync(search);
            return Ok(ApiResponse<List<SkillCoverageItemDto>>.Ok(skills));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get skills list");
            return StatusCode(500, ApiResponse<List<SkillCoverageItemDto>>.Fail("Error retrieving skills."));
        }
    }

    [HttpGet("skills/{name}")]
    public async Task<ActionResult<ApiResponse<SkillCoverageItemDto>>> GetSkillByName(string name)
    {
        try
        {
            var skill = await _service.GetSkillByNameAsync(name);
            if (skill == null)
            {
                return NotFound(ApiResponse<SkillCoverageItemDto>.Fail($"Skill '{name}' not found."));
            }
            return Ok(ApiResponse<SkillCoverageItemDto>.Ok(skill));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get skill {Name}", name);
            return StatusCode(500, ApiResponse<SkillCoverageItemDto>.Fail("Error retrieving skill."));
        }
    }

    [HttpPost("skills")]
    public async Task<ActionResult<ApiResponse<SkillCoverageItemDto>>> AddSkill(
        [FromBody] CreateSkillRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(ApiResponse<SkillCoverageItemDto>.Fail("Skill name cannot be empty."));
            }

            var skill = await _service.AddSkillAsync(request);
            return Ok(ApiResponse<SkillCoverageItemDto>.Ok(skill, "Skill added successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add skill");
            return StatusCode(500, ApiResponse<SkillCoverageItemDto>.Fail("Error adding skill."));
        }
    }

    [HttpDelete("skills/{name}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveSkill(string name)
    {
        try
        {
            var removed = await _service.RemoveSkillAsync(name);
            if (!removed)
            {
                return NotFound(ApiResponse<bool>.Fail($"Skill '{name}' not found."));
            }
            return Ok(ApiResponse<bool>.Ok(true, $"Skill '{name}' removed successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove skill {Name}", name);
            return StatusCode(500, ApiResponse<bool>.Fail("Error removing skill."));
        }
    }

    [HttpGet("forecast")]
    public async Task<ActionResult<ApiResponse<List<AvailabilityForecastDto>>>> GetForecast(
        [FromQuery] string? area)
    {
        try
        {
            var forecast = await _service.GetForecastAsync(area);
            return Ok(ApiResponse<List<AvailabilityForecastDto>>.Ok(forecast));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get availability forecast");
            return StatusCode(500, ApiResponse<List<AvailabilityForecastDto>>.Fail("Error retrieving forecast."));
        }
    }

    [HttpPost("requests")]
    public async Task<ActionResult<ApiResponse<AvailabilityRequestDto>>> SubmitRequest(
        [FromBody] AvailabilityRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Skill) || string.IsNullOrWhiteSpace(request.Location))
            {
                return BadRequest(ApiResponse<AvailabilityRequestDto>.Fail("Skill and location are required."));
            }

            var created = await _service.SubmitAvailabilityRequestAsync(request);
            return Ok(ApiResponse<AvailabilityRequestDto>.Ok(created, "Availability request submitted successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit availability request");
            return StatusCode(500, ApiResponse<AvailabilityRequestDto>.Fail("Error submitting request."));
        }
    }

    [HttpGet("requests")]
    public async Task<ActionResult<ApiResponse<List<AvailabilityRequestDto>>>> GetRequests()
    {
        try
        {
            var list = await _service.GetAvailabilityRequestsAsync();
            return Ok(ApiResponse<List<AvailabilityRequestDto>>.Ok(list));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get availability requests");
            return StatusCode(500, ApiResponse<List<AvailabilityRequestDto>>.Fail("Error retrieving requests."));
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportCsv()
    {
        try
        {
            var bytes = await _service.ExportSkillsCsvAsync();
            var fileName = $"carelink-skills-availability-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export skills CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting CSV."));
        }
    }
}
