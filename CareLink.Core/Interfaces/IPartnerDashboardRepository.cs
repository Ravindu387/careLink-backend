using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public interface IPartnerDashboardRepository
{
    Task<PartnerDashboardData> GetDashboardDataAsync(
        string? partnerId = null,
        CancellationToken cancellationToken = default);

    Task<PartnerActivityItem> LogActivityAsync(
        PartnerActivityItem activity,
        CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
