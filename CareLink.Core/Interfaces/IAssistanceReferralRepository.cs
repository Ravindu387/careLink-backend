using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public class AssistanceReferralFilterParams
{
    public string? Search { get; set; }
    public string? Tab { get; set; } // All, Needs action, Active, Completed, Archived
    public string? Crisis { get; set; }
    public string? Priority { get; set; }
    public string? Sector { get; set; }
    public string? Status { get; set; }
}

public interface IAssistanceReferralRepository
{
    Task<IReadOnlyList<AssistanceReferral>> GetAllAsync(
        AssistanceReferralFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<AssistanceReferral?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<AssistanceReferral> AddAsync(
        AssistanceReferral referral,
        CancellationToken cancellationToken = default);

    Task<AssistanceReferral?> UpdateAsync(
        AssistanceReferral referral,
        CancellationToken cancellationToken = default);

    Task<AssistanceReferral?> UpdateStatusAsync(
        string id,
        string status,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task<AssistanceReferral?> AssignTeamAsync(
        string id,
        string assignedTeam,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<AssistanceReferralMetrics> GetMetricsAsync(
        CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(
        CancellationToken cancellationToken = default);
}
