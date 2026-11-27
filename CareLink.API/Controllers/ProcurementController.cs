using Microsoft.AspNetCore.Authorization;
namespace CareLink.API.Controllers;

using System.Text;
using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProcurementController : ControllerBase
{
    private readonly IProcurementService _procurementService;

    public ProcurementController(IProcurementService procurementService)
    {
        _procurementService = procurementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] ProcurementFilterParams filters)
    {
        var result = await _procurementService.GetOrdersAsync(filters);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _procurementService.GetOrderByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _procurementService.GetSummaryAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest request)
    {
        var result = await _procurementService.CreateOrderAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdatePurchaseOrderStatusRequest request)
    {
        var result = await _procurementService.UpdateStatusAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _procurementService.DeleteOrderAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] ProcurementFilterParams filters)
    {
        var result = await _procurementService.GetOrdersAsync(filters);
        var orders = result.Data ?? [];

        var sb = new StringBuilder();
        sb.AppendLine("Purchase Order,Supplier,Items,Quantity,Value,Procurement Type,Delivery Due,Status,Notes");

        foreach (var item in orders)
        {
            sb.AppendLine($"\"{EscapeCsv(item.Id)}\",\"{EscapeCsv(item.Supplier)}\",\"{EscapeCsv(item.Items)}\",\"{EscapeCsv(item.Quantity)}\",\"{EscapeCsv(item.Value)}\",\"{EscapeCsv(item.Type)}\",\"{EscapeCsv(item.DeliveryDue)}\",\"{EscapeCsv(item.Status)}\",\"{EscapeCsv(item.Notes)}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"carelink-procurement-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\"", "\"\"");
    }
}
