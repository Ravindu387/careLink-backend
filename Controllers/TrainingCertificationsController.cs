using CareLink.Backend.Models;
using CareLink.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.Backend.Controllers;

[ApiController]
[Route("api/volunteer/training")]
[Route("api/volunteer/training-certifications")]
[Route("api/training-certifications")]
public class TrainingCertificationsController : ControllerBase
{
    private readonly ITrainingCertificationsService _service;
    private readonly ILogger<TrainingCertificationsController> _logger;

    public TrainingCertificationsController(
        ITrainingCertificationsService service,
        ILogger<TrainingCertificationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<TrainingOverviewDto>>> GetOverview()
    {
        try
        {
            var overview = await _service.GetOverviewAsync();
            return Ok(ApiResponse<TrainingOverviewDto>.Ok(overview, "Training and certifications overview retrieved."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get training overview");
            return StatusCode(500, ApiResponse<TrainingOverviewDto>.Fail("Error retrieving training overview."));
        }
    }

    [HttpGet("modules")]
    public async Task<ActionResult<ApiResponse<List<TrainingDto>>>> GetTrainings(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? requirement)
    {
        try
        {
            var trainings = await _service.GetTrainingsAsync(search, status, requirement);
            return Ok(ApiResponse<List<TrainingDto>>.Ok(trainings, "Trainings retrieved."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get trainings");
            return StatusCode(500, ApiResponse<List<TrainingDto>>.Fail("Error retrieving trainings."));
        }
    }

    [HttpGet("modules/{id}")]
    public async Task<ActionResult<ApiResponse<TrainingDto>>> GetTrainingById(string id)
    {
        try
        {
            var training = await _service.GetTrainingByIdAsync(id);
            if (training == null)
            {
                return NotFound(ApiResponse<TrainingDto>.Fail($"Training module '{id}' not found."));
            }
            return Ok(ApiResponse<TrainingDto>.Ok(training));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get training {Id}", id);
            return StatusCode(500, ApiResponse<TrainingDto>.Fail("Error retrieving training module."));
        }
    }

    [HttpPost("modules")]
    public async Task<ActionResult<ApiResponse<TrainingDto>>> CreateTraining(
        [FromBody] CreateTrainingRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(ApiResponse<TrainingDto>.Fail("Training name is required."));
            }

            var created = await _service.CreateTrainingAsync(request);
            return Ok(ApiResponse<TrainingDto>.Ok(created, "Training module created successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create training module");
            return StatusCode(500, ApiResponse<TrainingDto>.Fail("Error creating training module."));
        }
    }

    [HttpPatch("modules/{id}/status")]
    public async Task<ActionResult<ApiResponse<TrainingDto>>> UpdateTrainingStatus(
        string id,
        [FromBody] UpdateTrainingStatusRequest request)
    {
        try
        {
            var updated = await _service.UpdateTrainingStatusAsync(id, request.Status);
            if (updated == null)
            {
                return NotFound(ApiResponse<TrainingDto>.Fail($"Training module '{id}' not found."));
            }
            return Ok(ApiResponse<TrainingDto>.Ok(updated, "Training module status updated."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update training status for {Id}", id);
            return StatusCode(500, ApiResponse<TrainingDto>.Fail("Error updating training status."));
        }
    }

    [HttpDelete("modules/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTraining(string id)
    {
        try
        {
            var deleted = await _service.DeleteTrainingAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<bool>.Fail($"Training module '{id}' not found."));
            }
            return Ok(ApiResponse<bool>.Ok(true, $"Training module '{id}' deleted successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete training {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Error deleting training module."));
        }
    }

    [HttpGet("modules/export")]
    public async Task<IActionResult> ExportTrainingsCsv([FromQuery] string? search, [FromQuery] string? status)
    {
        try
        {
            var bytes = await _service.ExportTrainingsCsvAsync(search, status);
            var fileName = $"carelink-trainings-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export trainings CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting trainings CSV."));
        }
    }

    [HttpGet("certificates")]
    public async Task<ActionResult<ApiResponse<List<CertificateDto>>>> GetCertificates(
        [FromQuery] string? search,
        [FromQuery] string? status)
    {
        try
        {
            var certificates = await _service.GetCertificatesAsync(search, status);
            return Ok(ApiResponse<List<CertificateDto>>.Ok(certificates, "Certificates retrieved."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get certificates");
            return StatusCode(500, ApiResponse<List<CertificateDto>>.Fail("Error retrieving certificates."));
        }
    }

    [HttpGet("certificates/{id}")]
    public async Task<ActionResult<ApiResponse<CertificateDto>>> GetCertificateById(string id)
    {
        try
        {
            var cert = await _service.GetCertificateByIdAsync(id);
            if (cert == null)
            {
                return NotFound(ApiResponse<CertificateDto>.Fail($"Certificate '{id}' not found."));
            }
            return Ok(ApiResponse<CertificateDto>.Ok(cert));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get certificate {Id}", id);
            return StatusCode(500, ApiResponse<CertificateDto>.Fail("Error retrieving certificate."));
        }
    }

    [HttpPost("certificates")]
    public async Task<ActionResult<ApiResponse<CertificateDto>>> CreateCertificate(
        [FromBody] CreateCertificateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Volunteer) || string.IsNullOrWhiteSpace(request.Qualification))
            {
                return BadRequest(ApiResponse<CertificateDto>.Fail("Volunteer name and qualification are required."));
            }

            var created = await _service.CreateCertificateAsync(request);
            return Ok(ApiResponse<CertificateDto>.Ok(created, "Certificate uploaded successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create certificate");
            return StatusCode(500, ApiResponse<CertificateDto>.Fail("Error uploading certificate."));
        }
    }

    [HttpPatch("certificates/{id}/status")]
    public async Task<ActionResult<ApiResponse<CertificateDto>>> UpdateCertificateStatus(
        string id,
        [FromBody] UpdateCertificateStatusRequest request)
    {
        try
        {
            var updated = await _service.UpdateCertificateStatusAsync(id, request.Status);
            if (updated == null)
            {
                return NotFound(ApiResponse<CertificateDto>.Fail($"Certificate '{id}' not found."));
            }
            return Ok(ApiResponse<CertificateDto>.Ok(updated, "Certificate status updated."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update certificate status for {Id}", id);
            return StatusCode(500, ApiResponse<CertificateDto>.Fail("Error updating certificate status."));
        }
    }

    [HttpDelete("certificates/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCertificate(string id)
    {
        try
        {
            var deleted = await _service.DeleteCertificateAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<bool>.Fail($"Certificate '{id}' not found."));
            }
            return Ok(ApiResponse<bool>.Ok(true, $"Certificate '{id}' deleted successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete certificate {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Error deleting certificate."));
        }
    }

    [HttpGet("certificates/export")]
    public async Task<IActionResult> ExportCertificatesCsv([FromQuery] string? search, [FromQuery] string? status)
    {
        try
        {
            var bytes = await _service.ExportCertificatesCsvAsync(search, status);
            var fileName = $"carelink-certificates-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export certificates CSV");
            return StatusCode(500, ApiResponse<string>.Fail("Error exporting certificates CSV."));
        }
    }
}
