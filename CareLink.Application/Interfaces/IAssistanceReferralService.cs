using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IAssistanceReferralService
{
    Task<IReadOnlyList<AssistanceReferralDto>> GetAllAsync(
        AssistanceReferralQueryParameters? query = null,
        CancellationToken cancellationToken = default);

    Task<AssistanceReferralDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<AssistanceReferralDto> CreateAsync(
        CreateAssistanceReferralDto dto,
        CancellationToken cancellationToken = default);

    Task<AssistanceReferralDto?> UpdateAsync(
        string id,
        UpdateAssistanceReferralDto dto,
        CancellationToken cancellationToken = default);

    Task<AssistanceReferralDto?> UpdateStatusAsync(
        string id,
        UpdateReferralStatusDto dto,
        CancellationToken cancellationToken = default);

    Task<AssistanceReferralDto?> AssignTeamAsync(
        string id,
        AssignReferralTeamDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<AssistanceReferralMetricsDto> GetMetricsAsync(
        CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(
        CancellationToken cancellationToken = default);
}
