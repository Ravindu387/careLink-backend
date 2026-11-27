using System.Security.Claims;
using CareLink.Application.Common;
using CareLink.Application.DTOs.Organizations;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Route("api/organizations")]
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin},{UserRoles.EmergencyCoordinator},{UserRoles.PartnerOfficer}")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    /// <summary>
    /// Retrieve paginated organizations with optional search, status, and type filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedListDto<OrganizationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizations([FromQuery] OrganizationQueryDto query)
    {
        var result = await _organizationService.GetOrganizationsAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve organization counts across statuses (Total, Active, Pending, Suspended, Verified).
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _organizationService.GetStatsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve available organization classification types.
    /// </summary>
    [HttpGet("types")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTypes()
    {
        var result = await _organizationService.GetOrganizationTypesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieve a specific organization by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _organizationService.GetByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Register a new partner or government response organization.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<OrganizationDto>.FailureResult("Validation failed.", errors));
        }

        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _organizationService.CreateOrganizationAsync(request, actor);
        if (!result.Success) return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update existing organization information.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrganization(Guid id, [FromBody] UpdateOrganizationRequestDto request)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _organizationService.UpdateOrganizationAsync(id, request, actor);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Toggle organization operational status (Active vs Suspended).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrganizationStatusRequestDto request)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _organizationService.UpdateStatusAsync(id, request.Status, actor);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Verify and approve an organization's credential submission.
    /// </summary>
    [HttpPost("{id:guid}/verify")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveVerification(Guid id)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _organizationService.ApproveVerificationAsync(id, actor);
        return Ok(result);
    }

    /// <summary>
    /// Record an administrative verification decision (Approve, Reject, or RequestInfo) for a Partner Organisation.
    /// </summary>
    [HttpPost("{id:guid}/review")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReviewOrganization(Guid id, [FromBody] OrganizationReviewDecisionRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<OrganizationDto>.FailureResult("Validation failed.", errors));
        }

        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _organizationService.ReviewOrganizationAsync(id, request, actor);
        if (!result.Success)
        {
            return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Securely download or view a sensitive Partner Organisation verification document (Admin & authorized reviewers only).
    /// </summary>
    [HttpGet("{id:guid}/documents/{documentId:guid}/download")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin},{UserRoles.EmergencyCoordinator}")]
    public async Task<IActionResult> DownloadDocument(Guid id, Guid documentId)
    {
        var doc = await _organizationService.GetDocumentForDownloadAsync(id, documentId);
        if (doc == null)
        {
            return NotFound(ApiResponse<object>.FailureResult("Verification document not found."));
        }

        return File(doc.Value.Content, doc.Value.ContentType, doc.Value.FileName);
    }

    /// <summary>
    /// Delete an organization when safe and unassigned.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.SystemAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteOrganization(Guid id)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _organizationService.DeleteOrganizationAsync(id, actor);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }
}
