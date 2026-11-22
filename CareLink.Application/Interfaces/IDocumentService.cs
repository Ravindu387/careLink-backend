namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IDocumentService
{
    Task<ApiResponse<IEnumerable<CitizenDocumentDto>>> GetDocumentsAsync(string? citizenName = null, string? category = null, string? search = null);
    Task<ApiResponse<CitizenDocumentDto>> GetDocumentByIdAsync(string id);
    Task<ApiResponse<CitizenDocumentDto>> CreateDocumentAsync(CreateDocumentRequest request);
    Task<ApiResponse<CitizenDocumentDto>> UpdateStatusAsync(string id, UpdateDocumentStatusRequest request);
    Task<ApiResponse<bool>> DeleteDocumentAsync(string id);
    Task<ApiResponse<DocumentSummaryDto>> GetSummaryAsync(string? citizenName = null);
}
