using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public interface ITeamMemberRepository
{
    Task<IReadOnlyList<TeamMember>> GetAllAsync(TeamMemberQuery? query = null, CancellationToken cancellationToken = default);
    Task<TeamMember?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<TeamContactsMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);
    Task<TeamMember> CreateAsync(TeamMember member, CancellationToken cancellationToken = default);
    Task<TeamMember?> UpdateAsync(string id, TeamMember member, CancellationToken cancellationToken = default);
    Task<TeamMember?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task ResetToDefaultsAsync(CancellationToken cancellationToken = default);
}
