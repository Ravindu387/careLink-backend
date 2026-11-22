using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IExpenditureService
{
    Task<(IEnumerable<ExpenditureDto> Items, int TotalCount)> GetPagedAsync(ExpenditureQueryParameters parameters);
    Task<IEnumerable<ExpenditureDto>> GetAllAsync();
    Task<ExpenditureSummaryDto> GetSummaryAsync();
    Task<ExpenditureDto?> GetByIdAsync(string id);
    Task<ExpenditureDto> CreateAsync(CreateExpenditureDto dto);
    Task<ExpenditureDto?> UpdateAsync(string id, UpdateExpenditureDto dto);
    Task<ExpenditureDto?> UpdateVerificationAsync(string id, string verification);
    Task<ExpenditureDto?> UpdatePaymentStatusAsync(string id, string paymentStatus);
    Task<bool> DeleteAsync(string id);
    Task<int> BulkActionAsync(BulkExpenditureActionDto dto);
}
