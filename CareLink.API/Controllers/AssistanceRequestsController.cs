using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminDtos = CareLink.Application.DTOs.AssistanceRequests;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/assistance-requests")]
[Route("api/assistancerequests")]
public class AssistanceRequestsController : ControllerBase
{
    private readonly IAssistanceRequestService _service;
    private readonly ICitizenRequestService? _citizenRequestService;
    private readonly CareLink.Backend.Services.IMyCasesService? _myCasesService;
    private readonly CareLink.Backend.Services.ICaseManagementService? _caseManagementService;
    private readonly ILogger<AssistanceRequestsController> _logger;

    public AssistanceRequestsController(
        IAssistanceRequestService service,
        ILogger<AssistanceRequestsController> logger,
        ICitizenRequestService? citizenRequestService = null,
        CareLink.Backend.Services.IMyCasesService? myCasesService = null,
        CareLink.Backend.Services.ICaseManagementService? caseManagementService = null)
    {
        _service = service;
        _logger = logger;
        _citizenRequestService = citizenRequestService;
        _myCasesService = myCasesService;
        _caseManagementService = caseManagementService;
    }

    /// <summary>
    /// Retrieves assistance requests. Supports both direct list retrieval and paginated administrative queries.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AssistanceRequestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedListDto<AdminDtos.AssistanceRequestDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] AssistanceRequestQueryParameters query,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        if (pageNumber.HasValue || pageSize.HasValue)
        {
            var adminQuery = new AdminDtos.AssistanceRequestQueryDto
            {
                Search = query.Search,
                Status = query.Status,
                Priority = query.Priority,
                Category = query.Category,
                Crisis = query.CrisisEvent,
                PageNumber = pageNumber ?? 1,
                PageSize = pageSize ?? 10
            };

            var adminResult = await _service.GetRequestsAsync(adminQuery);
            return Ok(adminResult);
        }

        var items = await _service.GetAllAsync(query, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Aggregates and returns real-time assistance request operational metrics.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(AssistanceRequestMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieve summary statistics across request statuses and critical triage indicators (Admin).
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<AdminDtos.AssistanceRequestStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _service.GetStatsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single assistance request by unique reference ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AssistanceRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { error = $"Assistance request '{id}' was not found." });
        }

        return Ok(item);
    }

    /// <summary>
    /// Submits and registers a new assistance request into the active response pipeline, automatically running AI triage evaluation.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AssistanceRequestResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CareLink.Application.DTOs.CreateAssistanceRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Assistance request {Id} registered for {Category} at {Location}",
            result.Id, result.Category, result.Location);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing assistance request.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AssistanceRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] CareLink.Application.DTOs.UpdateAssistanceRequestDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Assistance request '{id}' was not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Assigns or reassigns an assistance request to a response team and organisation.
    /// </summary>
    [HttpPatch("{id}/assign")]
    [ProducesResponseType(typeof(AssistanceRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(
        string id,
        [FromBody] AssignAssistanceRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.AssignAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Assistance request '{id}' was not found." });
        }

        _logger.LogInformation("Assistance request {Id} assigned to {Assigned} ({Organisation})",
            id, dto.Assigned, dto.Organisation ?? updated.Organisation);

        return Ok(updated);
    }

    /// <summary>
    /// Updates the operational status of an assistance request.
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(AssistanceRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        string id,
        [FromBody] UpdateAssistanceRequestStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var validStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Submitted", "Under Review", "Assigned", "In Progress", "Escalated", "Completed", "Cancelled", "Awaiting review", "Verified", "Rejected"
        };

        if (!validStatuses.Contains(dto.Status))
        {
            return BadRequest(new { error = $"Status must be one of: {string.Join(", ", validStatuses)}." });
        }

        var updated = await _service.UpdateStatusAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Assistance request '{id}' was not found." });
        }

        if (_citizenRequestService != null)
        {
            await _citizenRequestService.UpdateStatusAsync(id, dto.Status);
        }

        if (_myCasesService != null)
        {
            await _myCasesService.UpdateStatusAsync(id, new CareLink.Backend.Models.UpdateMyCaseStatusRequest
            {
                Status = dto.Status
            });
        }

        if (_caseManagementService != null)
        {
            await _caseManagementService.UpdateStatusAsync(id, dto.Status);
        }

        _logger.LogInformation("Assistance request {Id} status updated to {Status}", id, dto.Status);
        return Ok(updated);
    }

    /// <summary>
    /// Overrides or escalates the priority level of an assistance request with justification (Admin / Coordinator).
    /// </summary>
    [HttpPatch("{id}/priority")]
    [ProducesResponseType(typeof(ApiResponse<AdminDtos.AssistanceRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePriority(
        string id,
        [FromBody] AdminDtos.UpdateRequestPriorityDto dto)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Coordinator";
        var result = await _service.UpdatePriorityAsync(id, dto, actor);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Deletes an assistance request.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { error = $"Assistance request '{id}' was not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Resets assistance requests and metrics back to initial seed data.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _service.ResetDataAsync(cancellationToken);
        var metrics = await _service.GetMetricsAsync(cancellationToken);
        return Ok(new { message = "Assistance requests reset to default baseline.", metrics });
    }
}
