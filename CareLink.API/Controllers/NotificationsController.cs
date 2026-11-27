using System.Security.Claims;
using System.Text.Json;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.DTOs.Users;
using CareLink.Application.Interfaces;
using CareLink.Core.Constants;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminDtos = CareLink.Application.DTOs.Notifications;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
[Route("api/citizen/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ICitizenNotificationService _citizenService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService service,
        ICitizenNotificationService citizenService,
        ILogger<NotificationsController> logger)
    {
        _service = service;
        _citizenService = citizenService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves notifications. Supports paged coordinator queries, administrative queries, shelter/funding filters, and citizen notifications.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedNotificationsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedListDto<AdminDtos.NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] NotificationQueryParameters query,
        [FromQuery] string? type,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        [FromQuery] bool? paged,
        [FromQuery] string? citizenName,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(citizenName) || HttpContext.Request.Path.Value?.Contains("/citizen/") == true)
        {
            var result = await _citizenService.GetNotificationsAsync(citizenName, category, query.UnreadOnly, query.Search);
            return Ok(result);
        }

        if (!string.IsNullOrWhiteSpace(type) || pageNumber.HasValue || pageSize.HasValue)
        {
            var adminQuery = new AdminDtos.NotificationQueryDto
            {
                Search = query.Search,
                Type = type,
                UnreadOnly = query.UnreadOnly ?? false,
                PageNumber = pageNumber ?? query.Page,
                PageSize = pageSize ?? query.PageSize
            };

            var adminResult = await _service.GetNotificationsAsync(adminQuery);
            return Ok(adminResult);
        }

        var effectiveRole = DetermineRole(query.Role);
        query.Role = effectiveRole;

        if (paged == false)
        {
            var list = await _service.GetAllAsync(query, cancellationToken);
            return Ok(list);
        }

        var res = await _service.GetPagedNotificationsAsync(query, cancellationToken);
        return Ok(res);
    }

    /// <summary>
    /// Aggregates and returns real-time notification operational metrics.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(NotificationMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(
        [FromQuery] string? role,
        CancellationToken cancellationToken)
    {
        var effectiveRole = DetermineRole(role);
        var metrics = await _service.GetMetricsAsync(effectiveRole, cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieve notification count statistics across unread, critical, high-priority, and domain categories (Admin).
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<AdminDtos.NotificationStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _service.GetStatsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets aggregated summary counts for notifications (Shelter & Funding, and Citizen).
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] string? role = "ShelterAdmin",
        [FromQuery] string? citizenName = null,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(citizenName) || HttpContext.Request.Path.Value?.Contains("/citizen/") == true)
        {
            var citizenSummary = await _citizenService.GetSummaryAsync(citizenName);
            return Ok(citizenSummary);
        }

        var summary = await _service.GetSummaryAsync(role, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets quick unread count for badge indicators.
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<object>> GetUnreadCount(
        [FromQuery] string? role = "ShelterAdmin",
        CancellationToken cancellationToken = default)
    {
        var count = await _service.GetUnreadCountAsync(role, cancellationToken);
        return Ok(new { count });
    }

    /// <summary>
    /// Retrieves all unread notifications.
    /// </summary>
    [HttpGet("unread")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnread(
        [FromQuery] string? role,
        CancellationToken cancellationToken)
    {
        var effectiveRole = DetermineRole(role);
        var query = new NotificationQueryParameters { Unread = true, Role = effectiveRole };
        var unread = await _service.GetAllAsync(query, cancellationToken);
        return Ok(unread);
    }

    /// <summary>
    /// Retrieves user notification preferences.
    /// </summary>
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(
        [FromQuery] string? role,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(role))
        {
            var p = await _service.GetPreferencesAsync(role, cancellationToken);
            return Ok(p);
        }

        var prefs = await _service.GetPreferencesAsync(cancellationToken);
        return Ok(prefs);
    }

    /// <summary>
    /// Updates user notification preferences.
    /// </summary>
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateNotificationPreferencesDto dto,
        [FromQuery] string? role,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var prefDto = new UpdateNotificationPreferenceDto
            {
                Critical = dto.Critical ?? true,
                Email = dto.Email ?? false,
                InApp = dto.Push ?? true,
                Sms = false,
                Requests = dto.Referrals ?? true,
                Inventory = dto.Resources ?? true,
                System = dto.Reports ?? false,
                Verification = true,
                Campaigns = true,
                Reports = dto.Reports ?? true
            };
            var saved = await _service.SavePreferencesAsync(role, prefDto, cancellationToken);
            return Ok(saved);
        }

        var updated = await _service.UpdatePreferencesAsync(dto, cancellationToken);
        _logger.LogInformation("Notification preferences updated.");
        return Ok(updated);
    }

    /// <summary>
    /// Retrieves a single notification by unique id.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        if (HttpContext.Request.Path.Value?.Contains("/citizen/") == true)
        {
            var citizenRes = await _citizenService.GetNotificationByIdAsync(id);
            if (!citizenRes.Success)
            {
                return NotFound(citizenRes);
            }
            return Ok(citizenRes);
        }

        if (int.TryParse(id, out var intId))
        {
            var itemById = await _service.GetNotificationByIdAsync(intId, cancellationToken);
            if (itemById != null) return Ok(itemById);
        }

        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item != null)
        {
            return Ok(item);
        }

        var fallback = await _citizenService.GetNotificationByIdAsync(id);
        if (fallback.Success)
        {
            return Ok(fallback);
        }

        return NotFound(new { message = $"Notification '{id}' not found." });
    }

    /// <summary>
    /// Marks a specific notification as read.
    /// </summary>
    [HttpPatch("{id}/read")]
    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(
        string id,
        CancellationToken cancellationToken)
    {
        if (HttpContext.Request.Path.Value?.Contains("/citizen/") == true)
        {
            var res = await _citizenService.MarkAsReadAsync(id);
            if (!res.Success)
            {
                return NotFound(res);
            }
            return Ok(res);
        }

        if (int.TryParse(id, out var intId))
        {
            var successInt = await _service.MarkAsReadAsync(intId, cancellationToken);
            if (successInt) return Ok(new { id, unread = false, message = "Notification marked as read." });
        }

        var success = await _service.MarkReadAsync(id, cancellationToken);
        if (success)
        {
            return Ok(new { id, unread = false, message = "Notification marked as read." });
        }

        var cResult = await _citizenService.MarkAsReadAsync(id);
        if (cResult.Success)
        {
            return Ok(cResult);
        }

        return NotFound(new { message = $"Notification '{id}' not found." });
    }


    /// <summary>
    /// Marks all notifications as read.
    /// </summary>
    [HttpPost("mark-all-read")]
    [HttpPatch("mark-all-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllRead(
        [FromQuery] string? role,
        [FromQuery] string? citizenName,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(citizenName) || HttpContext.Request.Path.Value?.Contains("/citizen/") == true)
        {
            var citizenRes = await _citizenService.MarkAllAsReadAsync(citizenName);
            return Ok(citizenRes);
        }

        var effectiveRole = DetermineRole(role);
        var updatedCount = await _service.MarkAllReadAsync(effectiveRole, cancellationToken);
        _logger.LogInformation("Marked all notifications as read ({Count} updated) for role {Role}.", updatedCount, effectiveRole ?? "all");
        return Ok(new { readCount = updatedCount, count = updatedCount, message = $"{updatedCount} notifications marked as read." });
    }

    /// <summary>
    /// Marks all notifications as read (alternative route).
    /// </summary>
    [HttpPost("mark-all-as-read")]
    public Task<IActionResult> MarkAllAsRead(
        [FromQuery] string? role,
        [FromQuery] string? citizenName,
        CancellationToken cancellationToken)
    {
        return MarkAllRead(role, citizenName, cancellationToken);
    }

    /// <summary>
    /// Creates and dispatches a new notification.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] JsonElement raw,
        CancellationToken cancellationToken)
    {
        var jsonText = raw.GetRawText();

        if (raw.TryGetProperty("citizenName", out _) || HttpContext.Request.Path.Value?.Contains("/citizen/") == true)
        {
            var citizenReq = JsonSerializer.Deserialize<CreateNotificationRequest>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (citizenReq != null)
            {
                var result = await _citizenService.CreateNotificationAsync(citizenReq);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
            }
        }

        var dto = JsonSerializer.Deserialize<CreateNotificationDto>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new { error = "Notification title is required." });
        }

        try
        {
            var created = await _service.CreateNotificationAsync(dto, cancellationToken);
            _logger.LogInformation("Notification {Id} created for {Category}: {Title}",
                created.Id, created.Type, created.Title);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return StatusCode(500, new { message = "An error occurred while creating the notification." });
        }
    }

    /// <summary>
    /// Deletes or archives a notification.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken)
    {
        if (HttpContext.Request.Path.Value?.Contains("/citizen/") == true)
        {
            var cResult = await _citizenService.DeleteNotificationAsync(id);
            if (!cResult.Success)
            {
                return NotFound(cResult);
            }
            return Ok(cResult);
        }

        bool deleted;
        if (int.TryParse(id, out var intId))
        {
            deleted = await _service.DeleteNotificationAsync(intId, cancellationToken);
        }
        else
        {
            deleted = await _service.DeleteAsync(id, cancellationToken);
        }

        if (deleted)
        {
            return Ok(new { id, message = $"Notification '{id}' deleted successfully." });
        }

        var fallback = await _citizenService.DeleteNotificationAsync(id);
        if (fallback.Success)
        {
            return Ok(fallback);
        }

        return NotFound(new { message = $"Notification '{id}' not found." });
    }

    /// <summary>
    /// Execute batch operation across multiple notifications (Admin).
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BatchAction([FromBody] AdminDtos.BatchNotificationActionDto request)
    {
        var actor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System Administrator";
        var result = await _service.BatchActionAsync(request, actor);
        return Ok(result);
    }

    /// <summary>
    /// Resets all notifications and preferences back to default seed data.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetDefaultData(CancellationToken cancellationToken)
    {
        await _service.ResetDefaultDataAsync(cancellationToken);
        _logger.LogInformation("Notifications reset to default seed scenario.");
        return Ok(new { message = "Notifications reset to default seed scenario." });
    }

    private string? DetermineRole(string? explicitRole)
    {
        if (!string.IsNullOrWhiteSpace(explicitRole))
        {
            return explicitRole.Trim().ToLowerInvariant();
        }

        var path = Request.Path.Value ?? string.Empty;
        if (path.Contains("/partner/", StringComparison.OrdinalIgnoreCase))
        {
            return "partner";
        }

        return null;
    }
}
