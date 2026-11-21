namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;

    public DocumentService(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IEnumerable<CitizenDocumentDto>>> GetDocumentsAsync(
        string? citizenName = null,
        string? category = null,
        string? search = null)
    {
        var documents = await _repository.GetAllAsync(citizenName, category, search);
        var dtos = documents.Select(MapToDto).ToList();
        return ApiResponse<IEnumerable<CitizenDocumentDto>>.SuccessResponse(dtos, "Documents retrieved successfully");
    }

    public async Task<ApiResponse<CitizenDocumentDto>> GetDocumentByIdAsync(string id)
    {
        var document = await _repository.GetByIdAsync(id);
        if (document == null)
        {
            return ApiResponse<CitizenDocumentDto>.ErrorResponse($"Document with ID '{id}' was not found.");
        }

        return ApiResponse<CitizenDocumentDto>.SuccessResponse(MapToDto(document), "Document retrieved successfully");
    }

    public async Task<ApiResponse<CitizenDocumentDto>> CreateDocumentAsync(CreateDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiResponse<CitizenDocumentDto>.ErrorResponse("Document name is required.");
        }

        var fileName = string.IsNullOrWhiteSpace(request.FileName) ? $"{request.Name.Trim().ToLowerInvariant().Replace(' ', '_')}.pdf" : request.FileName.Trim();
        var fileSizeBytes = request.FileSizeBytes ?? 1048576; // Default ~1MB if not provided

        var document = new CitizenDocument
        {
            Name = request.Name.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "Identity" : request.Category.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? fileName : request.Description.Trim(),
            FileName = fileName,
            FileType = string.IsNullOrWhiteSpace(request.FileType) ? GetFileType(fileName) : request.FileType.Trim(),
            FileSizeBytes = fileSizeBytes,
            FileSizeFormatted = FormatFileSize(fileSizeBytes),
            FileData = request.FileData,
            Status = "Saved",
            VerificationNotes = "Document uploaded by citizen. Pending officer verification.",
            CitizenName = string.IsNullOrWhiteSpace(request.CitizenName) ? "Samira Khan" : request.CitizenName.Trim(),
            CitizenId = "CIT-001",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(document);
        return ApiResponse<CitizenDocumentDto>.SuccessResponse(MapToDto(created), "Document uploaded successfully");
    }

    public async Task<ApiResponse<CitizenDocumentDto>> UpdateStatusAsync(string id, UpdateDocumentStatusRequest request)
    {
        var document = await _repository.GetByIdAsync(id);
        if (document == null)
        {
            return ApiResponse<CitizenDocumentDto>.ErrorResponse($"Document with ID '{id}' was not found.");
        }

        document.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.VerificationNotes))
        {
            document.VerificationNotes = request.VerificationNotes;
        }

        var updated = await _repository.UpdateAsync(document);
        return ApiResponse<CitizenDocumentDto>.SuccessResponse(MapToDto(updated!), "Document status updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteDocumentAsync(string id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Document with ID '{id}' was not found.");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Document deleted successfully");
    }

    public async Task<ApiResponse<DocumentSummaryDto>> GetSummaryAsync(string? citizenName = null)
    {
        var summary = await _repository.GetSummaryAsync(citizenName);
        return ApiResponse<DocumentSummaryDto>.SuccessResponse(summary, "Document summary retrieved successfully");
    }

    private static CitizenDocumentDto MapToDto(CitizenDocument entity)
    {
        return new CitizenDocumentDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Category = entity.Category,
            Description = entity.Description,
            Date = entity.CreatedAt.ToLocalTime().ToString("dd MMM yyyy"),
            Status = entity.Status,
            FileName = entity.FileName,
            FileType = entity.FileType,
            FileSizeBytes = entity.FileSizeBytes,
            FileSizeFormatted = string.IsNullOrWhiteSpace(entity.FileSizeFormatted) ? FormatFileSize(entity.FileSizeBytes) : entity.FileSizeFormatted,
            FileUrl = entity.FileUrl,
            FileData = entity.FileData,
            VerificationNotes = entity.VerificationNotes,
            CitizenName = entity.CitizenName,
            CreatedAt = entity.CreatedAt
        };
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes <= 0) return "0 KB";
        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024.0:F0} KB";
        }
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    private static string GetFileType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }
}
