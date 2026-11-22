namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Route("api/resource-dashboard")]
public class ResourceDashboardController : ControllerBase
{
    private readonly IResourceDashboardService _dashboardService;

    public ResourceDashboardController(IResourceDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _dashboardService.GetDashboardDataAsync();
        return Ok(result);
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests([FromQuery] ResourceRequestQueryParams query)
    {
        var result = await _dashboardService.GetResourceRequestsAsync(query);
        return Ok(result);
    }

    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateDashboardResourceRequestDto dto)
    {
        var result = await _dashboardService.CreateResourceRequestAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("receipt")]
    public async Task<IActionResult> RecordReceipt([FromBody] RecordStockReceiptRequest request)
    {
        var result = await _dashboardService.RecordStockReceiptAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string format = "csv")
    {
        var file = await _dashboardService.ExportDashboardSnapshotAsync(format);
        return File(file.Bytes, file.ContentType, file.FileName);
    }
}
