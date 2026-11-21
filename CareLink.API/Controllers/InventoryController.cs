namespace CareLink.API.Controllers;

using System.Text;
using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetInventory([FromQuery] InventoryFilterParams filters)
    {
        var result = await _inventoryService.GetInventoryAsync(filters);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _inventoryService.GetInventoryByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _inventoryService.GetSummaryAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemRequest request)
    {
        var result = await _inventoryService.CreateInventoryItemAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateInventoryItemRequest request)
    {
        var result = await _inventoryService.UpdateInventoryItemAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPatch("{id}/adjust")]
    public async Task<IActionResult> AdjustStock(string id, [FromBody] AdjustStockRequest request)
    {
        var result = await _inventoryService.AdjustStockAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _inventoryService.DeleteInventoryItemAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] InventoryFilterParams filters)
    {
        var result = await _inventoryService.GetInventoryAsync(filters);
        var items = result.Data ?? [];

        var sb = new StringBuilder();
        sb.AppendLine("Resource ID,Resource,Category,Warehouse,On Hand,Reserved,In Transit,Available,Status,Unit");

        foreach (var item in items)
        {
            sb.AppendLine($"\"{EscapeCsv(item.Id)}\",\"{EscapeCsv(item.Resource)}\",\"{EscapeCsv(item.Category)}\",\"{EscapeCsv(item.Warehouse)}\",{item.OnHand},{item.Reserved},{item.InTransit},{item.Available},\"{EscapeCsv(item.Status)}\",\"{EscapeCsv(item.Unit)}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"carelink-inventory-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\"", "\"\"");
    }
}
