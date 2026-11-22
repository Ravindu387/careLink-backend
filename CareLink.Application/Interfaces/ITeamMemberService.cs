using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface ITeamMemberService
{
    Task<IReadOnlyList<TeamMemberDto>> GetAllAsync(TeamMemberQueryParameters? query = null, CancellationToken cancellationToken = default);
    Task<TeamMemberDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<TeamContactsMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);
    Task<TeamMemberDto> CreateAsync(CreateTeamMemberDto dto, CancellationToken cancellationToken = default);
    Task<TeamMemberDto?> UpdateAsync(string id, UpdateTeamMemberDto dto, CancellationToken cancellationToken = default);
    Task<TeamMemberDto?> UpdateStatusAsync(string id, UpdateTeamMemberStatusDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task ResetToDefaultsAsync(CancellationToken cancellationToken = default);
}
