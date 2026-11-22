using CareLink.Core.Entities;
using CareLink.Core.Models;

namespace CareLink.Core.Interfaces;

public interface IResidentRepository
{
    Task<Resident?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Resident>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Resident> Items, int TotalCount)> GetPagedAsync(ResidentQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<(int TotalRegistered, int TotalShelters, int FamilyGroupsCount, int SingleResidentsCount, int AssistanceNeedsCount, int CheckedOutTodayCount, int CheckedInTodayCount)> GetSummaryMetricsAsync(CancellationToken cancellationToken = default);
    Task<Resident> AddAsync(Resident resident, CancellationToken cancellationToken = default);
    Task<Resident?> UpdateAsync(Resident resident, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default);
    Task<bool> CheckoutResidentAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
