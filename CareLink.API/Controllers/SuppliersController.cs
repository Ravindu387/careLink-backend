namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSuppliers(
        [FromQuery] string? tab,
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] string? region,
        [FromQuery] string? verification,
        [FromQuery] string? status)
    {
        var result = await _supplierService.GetSuppliersAsync(tab, search, category, region, verification, status);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _supplierService.GetSupplierByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _supplierService.GetSummaryAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
    {
        var result = await _supplierService.CreateSupplierAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateSupplierStatusRequest request)
    {
        var result = await _supplierService.UpdateStatusAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _supplierService.DeleteSupplierAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }
}
