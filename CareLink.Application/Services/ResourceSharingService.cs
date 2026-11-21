using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class ResourceSharingService : IResourceSharingService
{
    private readonly IResourceSharingRepository _repository;

    public ResourceSharingService(IResourceSharingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ResourceSharingCommitmentDto>> GetAllAsync(
        ResourceSharingQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var coreQuery = new ResourceSharingQuery
        {
            Search = query.Search,
            Tab = query.Tab,
            Type = query.Type,
            Location = query.Location,
            Partner = query.Partner,
            Status = query.Status
        };

        var list = await _repository.GetAllAsync(coreQuery, cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<ResourceSharingCommitmentDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<ResourceSharingCommitmentDto> CreateAsync(
        CreateResourceSharingCommitmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new ResourceSharingCommitment
        {
            Resource = dto.Resource.Trim(),
            Quantity = dto.Quantity,
            Source = dto.Source.Trim(),
            Destination = dto.Destination.Trim(),
            Partner = string.IsNullOrWhiteSpace(dto.Partner) ? "Global Relief Network" : dto.Partner.Trim(),
            Scheduled = string.IsNullOrWhiteSpace(dto.Scheduled) ? "Not scheduled" : dto.Scheduled.Trim(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending Approval" : dto.Status.Trim(),
            Type = string.IsNullOrWhiteSpace(dto.Type) ? "Food" : dto.Type.Trim(),
            Location = string.IsNullOrWhiteSpace(dto.Location) ? "North District" : dto.Location.Trim(),
            Description = dto.Description,
            ContactPerson = dto.ContactPerson,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<ResourceSharingCommitmentDto?> UpdateAsync(
        string id,
        UpdateResourceSharingCommitmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Resource)) existing.Resource = dto.Resource.Trim();
        if (dto.Quantity.HasValue && dto.Quantity.Value > 0) existing.Quantity = dto.Quantity.Value;
        if (!string.IsNullOrWhiteSpace(dto.Source)) existing.Source = dto.Source.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Destination)) existing.Destination = dto.Destination.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Partner)) existing.Partner = dto.Partner.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Scheduled)) existing.Scheduled = dto.Scheduled.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Status)) existing.Status = dto.Status.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Type)) existing.Type = dto.Type.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Location)) existing.Location = dto.Location.Trim();
        if (dto.Description != null) existing.Description = dto.Description;
        if (dto.ContactPerson != null) existing.ContactPerson = dto.ContactPerson;

        existing.UpdatedAt = DateTimeOffset.UtcNow;
        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<ResourceSharingCommitmentDto?> UpdateStatusAsync(
        string id,
        string status,
        CancellationToken cancellationToken = default)
    {
        var updated = await _repository.UpdateStatusAsync(id, status, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<ResourceSharingMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var metrics = await _repository.GetMetricsAsync(cancellationToken);
        return new ResourceSharingMetricsDto
        {
            ActiveCommitments = metrics.ActiveCommitments,
            ActiveCommitmentsSubtitle = metrics.ActiveCommitmentsSubtitle,
            UnitsCommitted = metrics.UnitsCommitted,
            UnitsCommittedSubtitle = metrics.UnitsCommittedSubtitle,
            DeliveredThisWeek = metrics.DeliveredThisWeek,
            DeliveredThisWeekSubtitle = metrics.DeliveredThisWeekSubtitle,
            Exceptions = metrics.Exceptions,
            ExceptionsSubtitle = metrics.ExceptionsSubtitle,
            TotalCommitments = metrics.TotalCommitments,
            NeedsActionCount = metrics.NeedsActionCount,
            ActiveCount = metrics.ActiveCount,
            CompletedCount = metrics.CompletedCount,
            ArchivedCount = metrics.ArchivedCount,
            LastUpdated = metrics.LastUpdated
        };
    }

    public Task ResetAsync(CancellationToken cancellationToken = default)
    {
        return _repository.ResetAsync(cancellationToken);
    }

    private static ResourceSharingCommitmentDto MapToDto(ResourceSharingCommitment entity)
    {
        return new ResourceSharingCommitmentDto
        {
            Id = entity.Id,
            Resource = entity.Resource,
            Quantity = entity.Quantity,
            Source = entity.Source,
            Destination = entity.Destination,
            Partner = entity.Partner,
            Scheduled = entity.Scheduled,
            Status = entity.Status,
            Type = entity.Type,
            Location = entity.Location,
            Description = entity.Description,
            ContactPerson = entity.ContactPerson,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
