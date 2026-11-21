namespace CareLink.Application.Interfaces;

using CareLink.Application.DTOs;
using CareLink.Core.Entities;

public interface IDocumentRepository
{
    Task<IEnumerable<CitizenDocument>> GetAllAsync(string? citizenName = null, string? category = null, string? search = null);
    Task<CitizenDocument?> GetByIdAsync(string id);
    Task<CitizenDocument> CreateAsync(CitizenDocument document);
    Task<CitizenDocument?> UpdateAsync(CitizenDocument document);
    Task<bool> DeleteAsync(string id);
    Task<DocumentSummaryDto> GetSummaryAsync(string? citizenName = null);
}
