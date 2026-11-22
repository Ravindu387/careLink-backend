using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IStaffService
{
    Task<IEnumerable<StaffMemberDto>> GetAllStaffAsync(CancellationToken cancellationToken = default);
    Task<PagedResultDto<StaffMemberDto>> GetPagedStaffAsync(StaffQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<StaffMemberDto?> GetStaffByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<StaffMemberDto> CreateStaffAsync(CreateStaffMemberDto dto, CancellationToken cancellationToken = default);
    Task<StaffMemberDto?> UpdateStaffAsync(string id, UpdateStaffMemberDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteStaffAsync(string id, CancellationToken cancellationToken = default);
    Task<StaffSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ShiftCoverageDto>> GetCoverageAsync(string? shelter = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ShiftCoverageDto>> UpdateCoverageAsync(IEnumerable<ShiftCoverageDto> coverage, CancellationToken cancellationToken = default);
}
