namespace CareLink.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;

[ApiController]
[Route("api/documents")]
[Route("api/citizen/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDocuments(
        [FromQuery] string? citizenName,
        [FromQuery] string? category,
        [FromQuery] string? search)
    {
        var result = await _documentService.GetDocumentsAsync(citizenName, category, search);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string? citizenName)
    {
        var result = await _documentService.GetSummaryAsync(citizenName);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _documentService.GetDocumentByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDocumentRequest request)
    {
        var result = await _documentService.CreateDocumentAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateDocumentStatusRequest request)
    {
        var result = await _documentService.UpdateStatusAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _documentService.DeleteDocumentAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

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

        // Return a structured simulated verification summary document if no raw binary data was stored
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

        var textBytes = System.Text.Encoding.UTF8.GetBytes(content);
        return File(textBytes, "text/plain", $"{Path.GetFileNameWithoutExtension(doc.FileName)}_record.txt");
    }
}
