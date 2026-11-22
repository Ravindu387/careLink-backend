namespace CareLink.API.Controllers;

using System.Text;
using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class DispatchesController : ControllerBase
{
    private readonly IDispatchService _dispatchService;

    public DispatchesController(IDispatchService dispatchService)
    {
        _dispatchService = dispatchService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDispatches([FromQuery] DispatchFilterParams filters)
    {
        var result = await _dispatchService.GetDispatchesAsync(filters);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _dispatchService.GetDispatchByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _dispatchService.GetSummaryAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDispatchRequest request)
    {
        var result = await _dispatchService.CreateDispatchAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateDispatchStatusRequest request)
    {
        var result = await _dispatchService.UpdateStatusAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _dispatchService.DeleteDispatchAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] DispatchFilterParams filters)
    {
        var result = await _dispatchService.GetDispatchesAsync(filters);
        var dispatches = result.Data ?? [];

        var sb = new StringBuilder();
        sb.AppendLine("Dispatch ID,Route,Resource,Quantity,Vehicle,Driver,Departed,ETA,Status,Current Location,Notes");

        foreach (var item in dispatches)
        {
            sb.AppendLine($"\"{EscapeCsv(item.Id)}\",\"{EscapeCsv(item.Route)}\",\"{EscapeCsv(item.Resource)}\",\"{EscapeCsv(item.Quantity)}\",\"{EscapeCsv(item.Vehicle)}\",\"{EscapeCsv(item.Driver)}\",\"{EscapeCsv(item.Departed)}\",\"{EscapeCsv(item.Eta)}\",\"{EscapeCsv(item.Status)}\",\"{EscapeCsv(item.CurrentLocation)}\",\"{EscapeCsv(item.Notes)}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"carelink-dispatches-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\"", "\"\"");
    }
}
