using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IFundAllocationService
{
    Task<(IEnumerable<FundAllocationDto> Items, int TotalCount)> GetPagedAsync(FundAllocationQueryParameters parameters);
    Task<IEnumerable<FundAllocationDto>> GetAllAsync();
    Task<FundAllocationSummaryDto> GetSummaryAsync();
    Task<FundAllocationDto?> GetByIdAsync(string id);
    Task<FundAllocationDto> CreateAsync(CreateFundAllocationDto dto);
    Task<FundAllocationDto?> UpdateAsync(string id, UpdateFundAllocationDto dto);
    Task<FundAllocationDto?> UpdateApprovalAsync(string id, string approval);
    Task<FundAllocationDto?> UpdateDisbursementAsync(string id, string disbursement);
    Task<bool> DeleteAsync(string id);
    Task<int> BulkActionAsync(BulkFundAllocationActionDto dto);
}
