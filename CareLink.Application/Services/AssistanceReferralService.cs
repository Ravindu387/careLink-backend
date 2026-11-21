using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class AssistanceReferralService : IAssistanceReferralService
{
    private readonly IAssistanceReferralRepository _repository;

    public AssistanceReferralService(IAssistanceReferralRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AssistanceReferralDto>> GetAllAsync(
        AssistanceReferralQueryParameters? query = null,
        CancellationToken cancellationToken = default)
    {
        var filterParams = query == null ? null : new AssistanceReferralFilterParams
        {
            Search = query.Search,
            Tab = query.Tab,
            Crisis = query.Crisis,
            Priority = query.Priority,
            Sector = query.Sector,
            Status = query.Status
        };

        var items = await _repository.GetAllAsync(filterParams, cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<AssistanceReferralDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item == null ? null : MapToDto(item);
    }

    public async Task<AssistanceReferralDto> CreateAsync(
        CreateAssistanceReferralDto dto,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var referral = new AssistanceReferral
        {
            Id = $"REF-{Random.Shared.Next(1000, 9999)}",
            Assistance = dto.Assistance.Trim(),
            Location = dto.Location.Trim(),
            Affected = dto.Affected > 0 ? dto.Affected : 1,
            Priority = string.IsNullOrWhiteSpace(dto.Priority) ? "High" : dto.Priority.Trim(),
            Received = now.ToString("dd MMM - HH:mm"),
            Assigned = string.IsNullOrWhiteSpace(dto.Assigned) ? "Unassigned" : dto.Assigned.Trim(),
            Status = "Awaiting Acceptance",
            Crisis = string.IsNullOrWhiteSpace(dto.Crisis) ? "Monsoon Floods 2025" : dto.Crisis.Trim(),
            Sector = string.IsNullOrWhiteSpace(dto.Sector) ? "Shelter" : dto.Sector.Trim(),
            Description = dto.Description?.Trim(),
            Organisation = string.IsNullOrWhiteSpace(dto.Organisation) ? "CareLink Partner Office" : dto.Organisation.Trim(),
            ContactName = dto.ContactName?.Trim(),
            ContactPhone = dto.ContactPhone?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _repository.AddAsync(referral, cancellationToken);
        return MapToDto(created);
    }

    public async Task<AssistanceReferralDto?> UpdateAsync(
        string id,
        UpdateAssistanceReferralDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return null;
        }

        if (dto.Assistance != null) existing.Assistance = dto.Assistance.Trim();
        if (dto.Location != null) existing.Location = dto.Location.Trim();
        if (dto.Affected.HasValue && dto.Affected.Value > 0) existing.Affected = dto.Affected.Value;
        if (dto.Priority != null) existing.Priority = dto.Priority.Trim();
        if (dto.Status != null) existing.Status = dto.Status.Trim();
        if (dto.Assigned != null) existing.Assigned = dto.Assigned.Trim();
        if (dto.Crisis != null) existing.Crisis = dto.Crisis.Trim();
        if (dto.Sector != null) existing.Sector = dto.Sector.Trim();
        if (dto.Description != null) existing.Description = dto.Description.Trim();
        if (dto.Organisation != null) existing.Organisation = dto.Organisation.Trim();
        if (dto.ContactName != null) existing.ContactName = dto.ContactName.Trim();
        if (dto.ContactPhone != null) existing.ContactPhone = dto.ContactPhone.Trim();

        existing.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<AssistanceReferralDto?> UpdateStatusAsync(
        string id,
        UpdateReferralStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await _repository.UpdateStatusAsync(id, dto.Status, dto.Reason, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<AssistanceReferralDto?> AssignTeamAsync(
        string id,
        AssignReferralTeamDto dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await _repository.AssignTeamAsync(id, dto.Assigned, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<AssistanceReferralMetricsDto> GetMetricsAsync(
        CancellationToken cancellationToken = default)
    {
        var metrics = await _repository.GetMetricsAsync(cancellationToken);
        return new AssistanceReferralMetricsDto
        {
            OpenReferrals = metrics.OpenReferrals,
            AwaitingAction = metrics.AwaitingAction,
            CriticalPriority = metrics.CriticalPriority,
            AcceptedToday = metrics.AcceptedToday,
            PeopleSupported = metrics.PeopleSupported,
            AllCount = metrics.AllCount,
            NeedsActionCount = metrics.NeedsActionCount,
            ActiveCount = metrics.ActiveCount,
            CompletedCount = metrics.CompletedCount,
            ArchivedCount = metrics.ArchivedCount,
            LastUpdated = metrics.LastUpdated
        };
    }

    public async Task ResetToDefaultAsync(
        CancellationToken cancellationToken = default)
    {
        await _repository.ResetToDefaultAsync(cancellationToken);
    }

    private static AssistanceReferralDto MapToDto(AssistanceReferral entity)
    {
        return new AssistanceReferralDto
        {
            Id = entity.Id,
            Assistance = entity.Assistance,
            Location = entity.Location,
            Affected = entity.Affected,
            Priority = entity.Priority,
            Received = entity.Received,
            Assigned = entity.Assigned,
            Status = entity.Status,
            Crisis = entity.Crisis,
            Sector = entity.Sector,
            Description = entity.Description,
            Organisation = entity.Organisation,
            ContactName = entity.ContactName,
            ContactPhone = entity.ContactPhone,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            AcceptedAt = entity.AcceptedAt,
            CompletedAt = entity.CompletedAt
        };
    }
}
