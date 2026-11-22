using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IFlaggedTransactionService
{
    Task<(IEnumerable<FlaggedTransactionDto> Items, int TotalCount)> GetPagedAsync(FlaggedTransactionQueryParameters parameters);
    Task<IEnumerable<FlaggedTransactionDto>> GetAllAsync();
    Task<FlaggedTransactionSummaryDto> GetSummaryAsync();
    Task<FlaggedTransactionDto?> GetByIdAsync(string id);
    Task<FlaggedTransactionDto> CreateAsync(CreateFlaggedTransactionDto dto);
    Task<FlaggedTransactionDto?> UpdateStatusAsync(string id, UpdateFlaggedTransactionStatusDto dto);
    Task<FlaggedTransactionDto?> AssignInvestigatorAsync(string id, string investigator);
    Task<bool> DeleteAsync(string id);
}
