namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Route("api/notifications")]
[Route("api/citizen/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly ICitizenNotificationService _notificationService;

    public NotificationsController(ICitizenNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] string? citizenName,
        [FromQuery] string? category,
        [FromQuery] bool? unreadOnly,
        [FromQuery] string? search)
    {
        var result = await _notificationService.GetNotificationsAsync(citizenName, category, unreadOnly, search);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string? citizenName)
    {
        var result = await _notificationService.GetSummaryAsync(citizenName);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _notificationService.GetNotificationByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationRequest request)
    {
        var result = await _notificationService.CreateNotificationAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPatch("{id}/read")]
    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        var result = await _notificationService.MarkAsReadAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost("mark-all-read")]
    [HttpPatch("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead([FromQuery] string? citizenName)
    {
        var result = await _notificationService.MarkAllAsReadAsync(citizenName);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _notificationService.DeleteNotificationAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }
}
