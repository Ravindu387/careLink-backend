using Microsoft.AspNetCore.Authorization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/resource-contributions")]
public class ResourceContributionsController : ControllerBase
{
    private readonly IResourceContributionService _service;

    public ResourceContributionsController(IResourceContributionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] ResourceContributionQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll([FromQuery] ResourceContributionQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new { message = $"Resource contribution '{id}' was not found." });
        }
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResourceContributionDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateResourceContributionDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { message = $"Resource contribution '{id}' was not found." });
        }
        return Ok(updated);
    }

    [HttpPatch("{id}/verify")]
    [HttpPost("{id}/verify")]
    public async Task<IActionResult> Verify(string id, [FromBody] UpdateVerificationStatusDto? dto, CancellationToken cancellationToken)
    {
        var status = dto?.Verification ?? "Verified";
        var success = await _service.UpdateVerificationAsync(id, status, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = $"Resource contribution '{id}' was not found." });
        }
        return Ok(new { success = true, message = $"Resource contribution '{id}' verification updated to {status}." });
    }

    [HttpPatch("{id}/allocate")]
    [HttpPost("{id}/allocate")]
    public async Task<IActionResult> Allocate(string id, [FromBody] UpdateAllocationStatusDto? dto, CancellationToken cancellationToken)
    {
        var status = dto?.Allocation ?? "Allocated";
        var success = await _service.UpdateAllocationAsync(id, status, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = $"Resource contribution '{id}' was not found." });
        }
        return Ok(new { success = true, message = $"Resource contribution '{id}' allocation updated to {status}." });
    }

    [HttpPost("bulk-verify")]
    public async Task<IActionResult> BulkVerify([FromBody] BulkVerifyResourceContributionsDto dto, CancellationToken cancellationToken)
    {
        if (dto.Ids == null || !dto.Ids.Any())
        {
            return BadRequest(new { message = "At least one contribution ID must be provided." });
        }

        var count = await _service.BulkVerifyAsync(dto.Ids, cancellationToken);
        return Ok(new { success = true, count, message = $"{count} contributions verified successfully." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var success = await _service.DeleteAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = $"Resource contribution '{id}' was not found." });
        }
        return NoContent();
    }
}
