using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/expenditures")]
public class ExpendituresController : ControllerBase
{
    private readonly IExpenditureService _service;

    public ExpendituresController(IExpenditureService service)
    {
        _service = service;
    }

    // GET /api/expenditures?search=...&type=...&verification=...&paymentStatus=...&dateRange=...&minAmount=...&maxAmount=...&page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] ExpenditureQueryParameters parameters)
    {
        var (items, total) = await _service.GetPagedAsync(parameters);
        return Ok(new { items, totalCount = total, page = parameters.Page, pageSize = parameters.PageSize });
    }

    // GET /api/expenditures/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    // GET /api/expenditures/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _service.GetSummaryAsync();
        return Ok(summary);
    }

    // GET /api/expenditures/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = $"Expenditure '{id}' not found." });
        return Ok(item);
    }

    // POST /api/expenditures
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenditureDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/expenditures/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateExpenditureDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null) return NotFound(new { message = $"Expenditure '{id}' not found." });
        return Ok(updated);
    }

    // PATCH /api/expenditures/{id}/verification
    [HttpPatch("{id}/verification")]
    public async Task<IActionResult> UpdateVerification(string id, [FromBody] UpdateExpenditureStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var updated = await _service.UpdateVerificationAsync(id, dto.Status);
        if (updated == null) return NotFound(new { message = $"Expenditure '{id}' not found." });
        return Ok(updated);
    }

    // PATCH /api/expenditures/{id}/payment
    [HttpPatch("{id}/payment")]
    public async Task<IActionResult> UpdatePayment(string id, [FromBody] UpdateExpenditureStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var updated = await _service.UpdatePaymentStatusAsync(id, dto.Status);
            if (updated == null) return NotFound(new { message = $"Expenditure '{id}' not found." });
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/expenditures/bulk
    [HttpPost("bulk")]
    public async Task<IActionResult> BulkAction([FromBody] BulkExpenditureActionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (dto.Ids == null || dto.Ids.Count == 0)
            return BadRequest(new { message = "No IDs provided." });

        var count = await _service.BulkActionAsync(dto);
        return Ok(new { updated = count, message = $"{count} expenditure(s) updated." });
    }

    // DELETE /api/expenditures/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(new { message = $"Expenditure '{id}' not found." });
        return NoContent();
    }
}
