using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public interface IEmergencySidebarRepository
{
    Task<EmergencySidebarState> GetStateAsync(CancellationToken cancellationToken = default);

    Task<CoordinatorProfile> UpdateCoordinatorDutyStatusAsync(
        string status,
        CancellationToken cancellationToken = default);

    Task<ActiveCrisisSummary> SetActiveCrisisAsync(
        ActiveCrisisSummary summary,
        CancellationToken cancellationToken = default);

    Task<int> DecrementNotificationCountAsync(CancellationToken cancellationToken = default);

    Task ResetStateAsync(CancellationToken cancellationToken = default);
}
