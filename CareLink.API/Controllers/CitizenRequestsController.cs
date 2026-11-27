namespace CareLink.API.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Backend.Models;
using CareLink.Backend.Services;
using CareLink.Core.Constants;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

[ApiController]
[Authorize]
[Route("api/citizen/requests")]
[Route("api/requests")]
public class CitizenRequestsController : ControllerBase
{
    private readonly ICitizenRequestService _requestService;
    private readonly IMyCasesService? _myCasesService;
    private readonly ICaseManagementService? _caseManagementService;
    private readonly IAssistanceRequestRepository? _assistanceRequestRepository;

    public CitizenRequestsController(
        ICitizenRequestService requestService,
        IMyCasesService? myCasesService = null,
        ICaseManagementService? caseManagementService = null,
        IAssistanceRequestRepository? assistanceRequestRepository = null)
    {
        _requestService = requestService;
        _myCasesService = myCasesService;
        _caseManagementService = caseManagementService;
        _assistanceRequestRepository = assistanceRequestRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetRequests(
        [FromQuery] string? citizenName,
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? category)
    {
        if (IsRestrictedCitizenCaller(out var authName, out var authEmail))
        {
            if (!string.IsNullOrWhiteSpace(citizenName) && !DoesCitizenOwnName(citizenName, authName, authEmail))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    CareLink.Application.Common.ApiResponse<object>.ErrorResponse("Access denied: you cannot view another citizen's private requests."));
            }
            citizenName ??= authName ?? authEmail;
        }

        var result = await _requestService.GetRequestsAsync(citizenName, search, status, category);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string? citizenName)
    {
        if (IsRestrictedCitizenCaller(out var authName, out var authEmail))
        {
            if (!string.IsNullOrWhiteSpace(citizenName) && !DoesCitizenOwnName(citizenName, authName, authEmail))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    CareLink.Application.Common.ApiResponse<object>.ErrorResponse("Access denied: you cannot view another citizen's private request summary."));
            }
            citizenName ??= authName ?? authEmail;
        }

        var result = await _requestService.GetSummaryAsync(citizenName);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _requestService.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        if (IsRestrictedCitizenCaller(out var authName, out var authEmail) &&
            result.Data != null &&
            !DoesCitizenOwnName(result.Data.CitizenName, authName, authEmail))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                CareLink.Application.Common.ApiResponse<object>.ErrorResponse("Access denied: you cannot access another citizen's private request."));
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateCitizenHelpRequestPayload request, [FromQuery] string? citizenName)
    {
        if (IsRestrictedCitizenCaller(out var authName, out var authEmail))
        {
            var effectiveCitizen = authName ?? authEmail ?? citizenName;
            citizenName = effectiveCitizen;
            request.CitizenName = effectiveCitizen;
            request.ContactName = effectiveCitizen;
        }

        var result = await _requestService.CreateRequestAsync(request, citizenName);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        var created = result.Data!;
        var mappedPriority = created.ImmediateThreat || string.Equals(created.Priority, "Urgent", StringComparison.OrdinalIgnoreCase)
            ? "Critical"
            : string.Equals(created.Priority, "High", StringComparison.OrdinalIgnoreCase)
                ? "High"
                : "Medium";

        _myCasesService?.UpsertExternalCase(new MyCaseItem
        {
            Id = created.Id,
            Category = created.Category,
            Applicant = $"{created.CitizenName} - {created.PeopleCount} people",
            Location = created.Address,
            Priority = mappedPriority,
            Confidence = "92% confidence",
            Verification = "Evidence pending",
            Assigned = "You",
            Age = "Just now",
            Status = "Under Review",
            Event = "Flood Response",
            Description = created.Description,
            CreatedAt = DateTime.UtcNow
        });

        _caseManagementService?.UpsertExternalCase(new CaseItem
        {
            Id = created.Id,
            Category = created.Category,
            Applicant = $"{created.CitizenName} - {created.PeopleCount} people",
            Location = created.Address,
            Priority = mappedPriority,
            AiPriority = mappedPriority,
            Confidence = "92% confidence",
            Verification = "Evidence pending",
            Assigned = "You",
            Age = "Just now",
            Status = "Under Review",
            Description = created.Description,
            CreatedAt = DateTime.UtcNow
        });

        if (_assistanceRequestRepository != null)
        {
            await _assistanceRequestRepository.AddAsync(new AssistanceRequest
            {
                Id = created.Id,
                Category = created.Category,
                Location = created.Address,
                CrisisEvent = "Monsoon Floods 2025",
                PriorityLevel = mappedPriority,
                Confidence = 92,
                PriorityDisplay = $"{mappedPriority} · 92%",
                Verification = "Pending verification",
                Assigned = "Unassigned",
                Status = "Under Review",
                Organisation = "National Disaster Authority",
                Description = created.Description,
                AffectedPeopleCount = created.PeopleCount,
                IsLifeThreatening = created.ImmediateThreat,
                ContactName = created.CitizenName,
                ContactPhone = created.ContactPhone,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, result);
    }

    [HttpPatch("{id}/status")]
    [HttpPost("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateRequestStatusPayload request)
    {
        var result = await _requestService.UpdateStatusAsync(id, request.Status, request.Progress);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRequest(string id)
    {
        if (IsRestrictedCitizenCaller(out var authName, out var authEmail))
        {
            var existing = await _requestService.GetByIdAsync(id);
            if (existing.Success && existing.Data != null && !DoesCitizenOwnName(existing.Data.CitizenName, authName, authEmail))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    CareLink.Application.Common.ApiResponse<object>.ErrorResponse("Access denied: you cannot delete another citizen's private request."));
            }
        }

        var result = await _requestService.DeleteRequestAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    private bool IsRestrictedCitizenCaller(out string? authName, out string? authEmail)
    {
        authName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("fullName");
        authEmail = User.FindFirstValue(ClaimTypes.Email);

        if (User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var isStaffOrAdmin =
            User.IsInRole(UserRoles.Admin) ||
            User.IsInRole(UserRoles.SystemAdmin) ||
            User.IsInRole(UserRoles.CaseManager) ||
            User.IsInRole(UserRoles.IncidentManager) ||
            User.IsInRole(UserRoles.EmergencyCoordinator);

        return !isStaffOrAdmin && User.IsInRole(UserRoles.Citizen);
    }

    private static bool DoesCitizenOwnName(string? recordCitizenName, string? authName, string? authEmail)
    {
        if (string.IsNullOrWhiteSpace(recordCitizenName)) return true;
        var target = recordCitizenName.Trim();

        if (!string.IsNullOrWhiteSpace(authName) && target.Equals(authName.Trim(), StringComparison.OrdinalIgnoreCase))
            return true;

        if (!string.IsNullOrWhiteSpace(authEmail) && target.Equals(authEmail.Trim(), StringComparison.OrdinalIgnoreCase))
            return true;

        // Seeded default citizen@carelink.org ("Citizen User") also owns the baseline "Samira Khan" seed records
        var isDefaultSeedCitizen =
            string.Equals(authEmail, "citizen@carelink.org", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(authName, "Citizen User", StringComparison.OrdinalIgnoreCase);

        if (isDefaultSeedCitizen &&
            (target.Equals("Samira Khan", StringComparison.OrdinalIgnoreCase) ||
             target.Equals("Citizen User", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return false;
    }
}
