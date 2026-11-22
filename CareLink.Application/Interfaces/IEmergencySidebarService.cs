using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IEmergencySidebarService
{
    Task<EmergencySidebarStateDto> GetSidebarStateAsync(CancellationToken cancellationToken = default);

    Task<CoordinatorProfileDto> UpdateDutyStatusAsync(
        string dutyStatus,
        CancellationToken cancellationToken = default);

    Task<ActiveCrisisSummaryDto> SetActiveCrisisAsync(
        string crisisId,
        CancellationToken cancellationToken = default);

    Task<int> MarkNotificationReadAsync(CancellationToken cancellationToken = default);

    Task<EmergencySidebarStateDto> ResetSidebarStateAsync(CancellationToken cancellationToken = default);
}
