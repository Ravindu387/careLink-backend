namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<IActionResult> GetReports([FromQuery] ReportFilterParams filters)
    {
        var result = await _reportService.GetReportsAsync(filters);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _reportService.GetReportByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateReportRequest request)
    {
        var result = await _reportService.GenerateReportAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _reportService.DeleteReportAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("schedules")]
    public async Task<IActionResult> GetSchedules()
    {
        var result = await _reportService.GetSchedulesAsync();
        return Ok(result);
    }

    [HttpPost("schedule")]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateReportScheduleRequest request)
    {
        var result = await _reportService.CreateScheduleAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPatch("schedules/{id:int}/toggle")]
    public async Task<IActionResult> ToggleSchedule(int id)
    {
        var result = await _reportService.ToggleScheduleStatusAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpDelete("schedules/{id:int}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        var result = await _reportService.DeleteScheduleAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics([FromQuery] AnalyticsQueryParams query)
    {
        var result = await _reportService.GetAnalyticsDashboardAsync(query);
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] ReportFilterParams filters, [FromQuery] string format = "csv")
    {
        var file = await _reportService.ExportReportFileAsync(null, format, filters);
        return File(file.Bytes, file.ContentType, file.FileName);
    }

    [HttpGet("{id:int}/export")]
    public async Task<IActionResult> ExportSingle(int id, [FromQuery] string format = "csv")
    {
        var file = await _reportService.ExportReportFileAsync(id, format);
        return File(file.Bytes, file.ContentType, file.FileName);
    }
}
