using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareLink.API.Controllers;

[ApiController]
[Authorize]
[Route("api/documents")]
[Route("api/partner/documents")]
[Route("api/citizen/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IPartnerDocumentService _partnerService;
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IPartnerDocumentService partnerService,
        IDocumentService documentService,
        ILogger<DocumentsController> logger)
    {
        _partnerService = partnerService;
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves documents with optional filtering. Supports both Partner document queries and Citizen document queries.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? tab,
        [FromQuery] string? type,
        [FromQuery] string? owner,
        [FromQuery] string? visibility,
        [FromQuery] string? status,
        [FromQuery] string? citizenName,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(citizenName) || HttpContext.Request.Path.Value?.Contains("/citizen/") == true)
        {
            var citizenDocs = await _documentService.GetDocumentsAsync(citizenName, category, search);
            return Ok(citizenDocs);
        }

        var query = new PartnerDocumentQueryParameters
        {
            Search = search,
            Tab = tab,
            Type = type,
            Owner = owner,
            Visibility = visibility,
            Status = status
        };

        var results = await _partnerService.GetAllAsync(query, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Retrieves aggregate metrics for partner documents.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(PartnerDocumentMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await _partnerService.GetMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves summary counts for citizen documents.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string? citizenName)
    {
        var result = await _documentService.GetSummaryAsync(citizenName);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single document by its identifier.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        if (HttpContext.Request.Path.Value?.Contains("/citizen/") == true)
        {
            var citizenRes = await _documentService.GetDocumentByIdAsync(id);
            if (!citizenRes.Success)
            {
                return NotFound(citizenRes);
            }
            return Ok(citizenRes);
        }

        var partnerDoc = await _partnerService.GetByIdAsync(id, cancellationToken);
        if (partnerDoc != null)
        {
            return Ok(partnerDoc);
        }

        // Fallback to citizen document
        var fallback = await _documentService.GetDocumentByIdAsync(id);
        if (fallback.Success)
        {
            return Ok(fallback);
        }

        return NotFound(new { error = $"Document '{id}' was not found." });
    }

    /// <summary>
    /// Uploads and registers a new document (Partner or Citizen).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JsonElement raw, CancellationToken cancellationToken)
    {
        var jsonText = raw.GetRawText();

        if (raw.TryGetProperty("citizenName", out _) || raw.TryGetProperty("fileData", out _) || HttpContext.Request.Path.Value?.Contains("/citizen/") == true)
        {
            var req = JsonSerializer.Deserialize<CreateDocumentRequest>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (req != null)
            {
                var result = await _documentService.CreateDocumentAsync(req);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
            }
        }

        var partnerDto = JsonSerializer.Deserialize<CreatePartnerDocumentDto>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (partnerDto == null || string.IsNullOrWhiteSpace(partnerDto.Name))
        {
            return BadRequest(new { error = "Document name is required." });
        }

        var created = await _partnerService.CreateAsync(partnerDto, cancellationToken);
        _logger.LogInformation("Document {Id} uploaded by {Owner}: {Name}", created.Id, created.Owner, created.Name);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates document metadata and properties (Partner documents).
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdatePartnerDocumentDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _partnerService.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { error = $"Document '{id}' was not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Updates status of a document (supports both Partner and Citizen documents).
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        string id,
        [FromBody] JsonElement raw,
        CancellationToken cancellationToken)
    {
        var jsonText = raw.GetRawText();

        if (raw.TryGetProperty("verificationNotes", out _) || HttpContext.Request.Path.Value?.Contains("/citizen/") == true)
        {
            var citizenReq = JsonSerializer.Deserialize<UpdateDocumentStatusRequest>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (citizenReq != null)
            {
                var res = await _documentService.UpdateStatusAsync(id, citizenReq);
                if (!res.Success)
                {
                    return NotFound(res);
                }
                return Ok(res);
            }
        }

        var partnerReq = JsonSerializer.Deserialize<UpdatePartnerDocumentStatusDto>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (partnerReq != null)
        {
            var updated = await _partnerService.UpdateStatusAsync(id, partnerReq, cancellationToken);
            if (updated != null)
            {
                _logger.LogInformation("Document {Id} status updated to {Status}", id, partnerReq.Status);
                return Ok(updated);
            }
        }

        return NotFound(new { error = $"Document '{id}' was not found." });
    }

    /// <summary>
    /// Deletes a document record.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        if (HttpContext.Request.Path.Value?.Contains("/citizen/") == true)
        {
            var citizenRes = await _documentService.DeleteDocumentAsync(id);
            if (!citizenRes.Success)
            {
                return NotFound(citizenRes);
            }
            return Ok(citizenRes);
        }

        var success = await _partnerService.DeleteAsync(id, cancellationToken);
        if (success)
        {
            return NoContent();
        }

        var cRes = await _documentService.DeleteDocumentAsync(id);
        if (cRes.Success)
        {
            return Ok(cRes);
        }

        return NotFound(new { error = $"Document '{id}' was not found." });
    }

    /// <summary>
    /// Downloads document content as binary or simulated certified plain text record.
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(string id)
    {
        var result = await _documentService.GetDocumentByIdAsync(id);
        if (!result.Success || result.Data == null)
        {
            return NotFound(result);
        }

        var doc = result.Data;
        if (!string.IsNullOrWhiteSpace(doc.FileData))
        {
            try
            {
                var cleanBase64 = doc.FileData;
                var commaIndex = cleanBase64.IndexOf(',');
                if (commaIndex >= 0)
                {
                    cleanBase64 = cleanBase64.Substring(commaIndex + 1);
                }
                var bytes = Convert.FromBase64String(cleanBase64);
                return File(bytes, string.IsNullOrWhiteSpace(doc.FileType) ? "application/octet-stream" : doc.FileType, doc.FileName);
            }
            catch
            {
                // Fallback to text file below if base64 decoding fails
            }
        }

        var content = $"CareLink ReliefOS Official Document Record\n" +
                      $"===========================================\n" +
                      $"Document ID: {doc.Id}\n" +
                      $"Document Name: {doc.Name}\n" +
                      $"Category: {doc.Category}\n" +
                      $"Description: {doc.Description}\n" +
                      $"Citizen: {doc.CitizenName}\n" +
                      $"Status: {doc.Status}\n" +
                      $"Verification Notes: {doc.VerificationNotes}\n" +
                      $"Registered Date: {doc.Date}\n" +
                      $"Security: Certified for disaster relief verification.";

        var textBytes = Encoding.UTF8.GetBytes(content);
        return File(textBytes, "text/plain", $"{Path.GetFileNameWithoutExtension(doc.FileName)}_record.txt");
    }

    /// <summary>
    /// Resets partner documents to demonstration baseline dataset.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _partnerService.ResetToDefaultAsync(cancellationToken);
        var metrics = await _partnerService.GetMetricsAsync(cancellationToken);
        return Ok(new { message = "Documents reset to default state.", metrics });
    }
}
