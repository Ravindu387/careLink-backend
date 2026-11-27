using Microsoft.AspNetCore.Authorization;
namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Authorize]
[Route("api/resource-requests")]
public class ResourceRequestsController : ControllerBase
{
    private readonly IResourceRequestService _service;

    public ResourceRequestsController(IResourceRequestService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetRequests([FromQuery] ResourceRequestFilterParams filters)
    {
        var result = await _service.GetRequestsAsync(filters);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _service.GetSummaryAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetRequestByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResourceRequestPayload payload)
    {
        var result = await _service.CreateRequestAsync(payload);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateResourceRequestStatusPayload payload)
    {
        var result = await _service.UpdateRequestStatusAsync(id, payload);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _service.DeleteRequestAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] ResourceRequestFilterParams filters, [FromQuery] string format = "csv")
    {
        var file = await _service.ExportRequestsAsync(filters, format);
        return File(file.Bytes, file.ContentType, file.FileName);
    }
}
