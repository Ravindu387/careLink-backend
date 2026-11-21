using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IPartnerDashboardService
{
    Task<PartnerDashboardResponseDto> GetDashboardAsync(
        string? partnerId = null,
        CancellationToken cancellationToken = default);

    Task<PartnerActivityDto> LogActivityAsync(
        CreatePartnerActivityDto dto,
        CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
