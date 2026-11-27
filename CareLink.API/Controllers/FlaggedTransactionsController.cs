using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/flagged-transactions")]
public class FlaggedTransactionsController : ControllerBase
{
    private readonly IFlaggedTransactionService _service;

    public FlaggedTransactionsController(IFlaggedTransactionService service)
    {
        _service = service;
    }

    // GET /api/flagged-transactions?search=...&recordType=...&severity=...&status=...&dateSort=...&page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] FlaggedTransactionQueryParameters parameters)
    {
        var (items, total) = await _service.GetPagedAsync(parameters);
        return Ok(new { items, totalCount = total, page = parameters.Page, pageSize = parameters.PageSize });
    }

    // GET /api/flagged-transactions/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    // GET /api/flagged-transactions/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _service.GetSummaryAsync();
        return Ok(summary);
    }

    // GET /api/flagged-transactions/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = $"Flagged transaction '{id}' not found." });
        return Ok(item);
    }

    // POST /api/flagged-transactions
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFlaggedTransactionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PATCH /api/flagged-transactions/{id}/status
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateFlaggedTransactionStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var updated = await _service.UpdateStatusAsync(id, dto);
        if (updated == null) return NotFound(new { message = $"Flagged transaction '{id}' not found." });
        return Ok(updated);
    }

    // PATCH /api/flagged-transactions/{id}/assign
    [HttpPatch("{id}/assign")]
    public async Task<IActionResult> AssignInvestigator(string id, [FromBody] AssignInvestigatorDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Investigator))
            return BadRequest(new { message = "Investigator name is required." });
        var updated = await _service.AssignInvestigatorAsync(id, dto.Investigator);
        if (updated == null) return NotFound(new { message = $"Flagged transaction '{id}' not found." });
        return Ok(updated);
    }

    // DELETE /api/flagged-transactions/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(new { message = $"Flagged transaction '{id}' not found." });
        return NoContent();
    }
}

public class AssignInvestigatorDto
{
    public string Investigator { get; set; } = string.Empty;
}
