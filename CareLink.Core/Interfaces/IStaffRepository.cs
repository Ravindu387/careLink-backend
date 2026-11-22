using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IStaffRepository
{
    Task<IEnumerable<StaffMember>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<StaffMember> Items, int TotalCount)> GetPagedAsync(StaffQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<StaffMember?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<StaffMember> AddAsync(StaffMember staff, CancellationToken cancellationToken = default);
    Task<StaffMember?> UpdateAsync(StaffMember staff, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<(int TotalStaff, int OnDuty, int NextShift, int Unavailable)> GetSummaryMetricsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ShiftCoverage>> GetCoverageAsync(string? shelter = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ShiftCoverage>> UpdateCoverageAsync(IEnumerable<ShiftCoverage> coverage, CancellationToken cancellationToken = default);
}
