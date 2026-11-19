using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs.AssistanceRequests;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/assistance-requests")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin},{UserRoles.EmergencyCoordinator},{UserRoles.IncidentManager},{UserRoles.PartnerOfficer},{UserRoles.VolunteerCoordinator}")]
public class AssistanceRequestsController : ControllerBase
{
    private readonly IAssistanceRequestService _requestService;

    public AssistanceRequestsController(IAssistanceRequestService requestService)
    {
        _requestService = requestService;
    }

    /// <summary>
    /// Retrieve paginated assistance requests with optional crisis, status, priority, and category filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedListDto<AssistanceRequestDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRequests([FromQuery] AssistanceRequestQueryDto query)
    {
        var result = await _requestService.GetRequestsAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve summary statistics across request statuses and critical triage indicators.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<AssistanceRequestStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _requestService.GetStatsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve details of a specific assistance request by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AssistanceRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AssistanceRequestDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _requestService.GetByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Submit a new assistance request into the response tracking pipeline.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AssistanceRequestDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AssistanceRequestDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRequest([FromBody] CreateAssistanceRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<AssistanceRequestDto>.FailureResult("Validation failed.", errors));
        }

        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Coordinator";
        var result = await _requestService.CreateRequestAsync(request, actor);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update details of an existing assistance request.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AssistanceRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AssistanceRequestDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRequest(string id, [FromBody] UpdateAssistanceRequestDto request)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Coordinator";
        var result = await _requestService.UpdateRequestAsync(id, request, actor);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Update operational request status (e.g., Verified, Completed, Rejected with reason).
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<AssistanceRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AssistanceRequestDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateRequestStatusDto request)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Coordinator";
        var result = await _requestService.UpdateStatusAsync(id, request, actor);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Assign an assistance request to a response team or field officer.
    /// </summary>
    [HttpPost("{id}/assign")]
    [ProducesResponseType(typeof(ApiResponse<AssistanceRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AssistanceRequestDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignRequest(string id, [FromBody] AssignRequestDto request)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Coordinator";
        var result = await _requestService.AssignRequestAsync(id, request, actor);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Modify the operational response priority of an assistance request.
    /// </summary>
    [HttpPatch("{id}/priority")]
    [ProducesResponseType(typeof(ApiResponse<AssistanceRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AssistanceRequestDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePriority(string id, [FromBody] UpdateRequestPriorityDto request)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Coordinator";
        var result = await _requestService.UpdatePriorityAsync(id, request, actor);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Delete an assistance request record.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteRequest(string id)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Admin";
        var result = await _requestService.DeleteRequestAsync(id, actor);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
