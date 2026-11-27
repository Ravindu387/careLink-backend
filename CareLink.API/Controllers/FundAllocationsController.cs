using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/fund-allocations")]
public class FundAllocationsController : ControllerBase
{
    private readonly IFundAllocationService _service;

    public FundAllocationsController(IFundAllocationService service)
    {
        _service = service;
    }

    // GET /api/fund-allocations?search=...&source=...&approval=...&disbursement=...&dateRange=...&minAmount=...&maxAmount=...&page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] FundAllocationQueryParameters parameters)
    {
        var (items, total) = await _service.GetPagedAsync(parameters);
        return Ok(new { items, totalCount = total, page = parameters.Page, pageSize = parameters.PageSize });
    }

    // GET /api/fund-allocations/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    // GET /api/fund-allocations/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _service.GetSummaryAsync();
        return Ok(summary);
    }

    // GET /api/fund-allocations/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = $"Fund allocation '{id}' not found." });
        return Ok(item);
    }

    // POST /api/fund-allocations
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFundAllocationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/fund-allocations/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateFundAllocationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null) return NotFound(new { message = $"Fund allocation '{id}' not found." });
        return Ok(updated);
    }

    // PATCH /api/fund-allocations/{id}/approval
    [HttpPatch("{id}/approval")]
    public async Task<IActionResult> UpdateApproval(string id, [FromBody] UpdateFundAllocationStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var updated = await _service.UpdateApprovalAsync(id, dto.Status);
        if (updated == null) return NotFound(new { message = $"Fund allocation '{id}' not found." });
        return Ok(updated);
    }

    // PATCH /api/fund-allocations/{id}/disbursement
    [HttpPatch("{id}/disbursement")]
    public async Task<IActionResult> UpdateDisbursement(string id, [FromBody] UpdateFundAllocationStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var updated = await _service.UpdateDisbursementAsync(id, dto.Status);
            if (updated == null) return NotFound(new { message = $"Fund allocation '{id}' not found." });
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/fund-allocations/bulk
    [HttpPost("bulk")]
    public async Task<IActionResult> BulkAction([FromBody] BulkFundAllocationActionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (dto.Ids == null || dto.Ids.Count == 0)
            return BadRequest(new { message = "No IDs provided." });

        var count = await _service.BulkActionAsync(dto);
        return Ok(new { updated = count, message = $"{count} allocation(s) updated." });
    }

    // DELETE /api/fund-allocations/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(new { message = $"Fund allocation '{id}' not found." });
        return NoContent();
    }
}
