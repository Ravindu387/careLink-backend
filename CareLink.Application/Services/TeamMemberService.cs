using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class TeamMemberService : ITeamMemberService
{
    private readonly ITeamMemberRepository _repository;

    public TeamMemberService(ITeamMemberRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<TeamMemberDto>> GetAllAsync(TeamMemberQueryParameters? query = null, CancellationToken cancellationToken = default)
    {
        TeamMemberQuery? domainQuery = null;
        if (query != null)
        {
            domainQuery = new TeamMemberQuery
            {
                Search = query.Search,
                Tab = query.Tab,
                Role = query.Role,
                Sector = query.Sector,
                Location = query.Location,
                Status = query.Status
            };
        }

        var items = await _repository.GetAllAsync(domainQuery, cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<TeamMemberDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item == null ? null : MapToDto(item);
    }

    public async Task<TeamContactsMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var metrics = await _repository.GetMetricsAsync(cancellationToken);
        return new TeamContactsMetricsDto
        {
            ActiveRecords = metrics.ActiveRecords,
            ActiveRecordsSubtitle = metrics.ActiveRecordsSubtitle,
            NeedsReview = metrics.NeedsReview,
            NeedsReviewSubtitle = metrics.NeedsReviewSubtitle,
            Verified = metrics.Verified,
            VerifiedSubtitle = metrics.VerifiedSubtitle,
            DueSoon = metrics.DueSoon,
            DueSoonSubtitle = metrics.DueSoonSubtitle,
            TotalCount = metrics.TotalCount,
            NeedsActionCount = metrics.NeedsActionCount,
            ActiveCount = metrics.ActiveCount,
            CompletedCount = metrics.CompletedCount,
            ArchivedCount = metrics.ArchivedCount,
            LastUpdated = metrics.LastUpdated
        };
    }

    public async Task<TeamMemberDto> CreateAsync(CreateTeamMemberDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new TeamMember
        {
            Name = dto.Name.Trim(),
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "Field Team Lead" : dto.Role.Trim(),
            Sector = string.IsNullOrWhiteSpace(dto.Sector) ? "WASH" : dto.Sector.Trim(),
            Area = string.IsNullOrWhiteSpace(dto.Area) ? "North Ward" : dto.Area.Trim(),
            Contact = dto.Contact.Trim(),
            Email = dto.Email.Trim(),
            LastActive = string.IsNullOrWhiteSpace(dto.LastActive) ? "Invited" : dto.LastActive.Trim(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<TeamMemberDto?> UpdateAsync(string id, UpdateTeamMemberDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        var updated = new TeamMember
        {
            Id = existing.Id,
            Name = dto.Name != null ? dto.Name.Trim() : existing.Name,
            Role = dto.Role != null ? dto.Role.Trim() : existing.Role,
            Sector = dto.Sector != null ? dto.Sector.Trim() : existing.Sector,
            Area = dto.Area != null ? dto.Area.Trim() : existing.Area,
            Contact = dto.Contact != null ? dto.Contact.Trim() : existing.Contact,
            Email = dto.Email != null ? dto.Email.Trim() : existing.Email,
            LastActive = dto.LastActive != null ? dto.LastActive.Trim() : existing.LastActive,
            Status = dto.Status != null ? dto.Status.Trim() : existing.Status,
            Description = dto.Description != null ? dto.Description.Trim() : existing.Description,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var result = await _repository.UpdateAsync(id, updated, cancellationToken);
        return result == null ? null : MapToDto(result);
    }

    public async Task<TeamMemberDto?> UpdateStatusAsync(string id, UpdateTeamMemberStatusDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _repository.UpdateStatusAsync(id, dto.Status.Trim(), cancellationToken);
        return result == null ? null : MapToDto(result);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task ResetToDefaultsAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetToDefaultsAsync(cancellationToken);
    }

    private static TeamMemberDto MapToDto(TeamMember entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Role = entity.Role,
        Sector = entity.Sector,
        Area = entity.Area,
        Contact = entity.Contact,
        Email = entity.Email,
        LastActive = entity.LastActive,
        Status = entity.Status,
        Description = entity.Description,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
