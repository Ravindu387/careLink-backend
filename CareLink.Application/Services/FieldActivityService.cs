using System.Globalization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class FieldActivityService : IFieldActivityService
{
    private readonly IFieldActivityRepository _repository;
    private static int _idCounter = 1045;
    private static readonly object _lock = new();

    public FieldActivityService(IFieldActivityRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<FieldActivityDto>> GetAllAsync(
        FieldActivityQueryParameters? query = null,
        CancellationToken cancellationToken = default)
    {
        var filter = query == null ? null : new FieldActivityFilterParams
        {
            Search = query.Search,
            Tab = query.Tab,
            Sector = query.Sector,
            Location = query.Location,
            Team = query.Team,
            Status = query.Status
        };

        var items = await _repository.GetAllAsync(filter, cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<FieldActivityDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item == null ? null : MapToDto(item);
    }

    public async Task<FieldActivityDto> CreateAsync(
        CreateFieldActivityDto dto,
        CancellationToken cancellationToken = default)
    {
        string id;
        lock (_lock)
        {
            _idCounter++;
            id = $"ACT-{_idCounter}";
        }

        var now = DateTimeOffset.UtcNow;
        var formattedSched = FormatScheduledDate(dto.Scheduled);

        var entity = new FieldActivity
        {
            Id = id,
            Name = dto.Name.Trim(),
            Location = dto.Location.Trim(),
            Scheduled = formattedSched,
            Team = string.IsNullOrWhiteSpace(dto.Team) ? "Field Team North" : dto.Team.Trim(),
            Target = dto.Target.Trim(),
            Sector = string.IsNullOrWhiteSpace(dto.Sector) ? "Food Security" : dto.Sector.Trim(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Scheduled" : dto.Status.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<FieldActivityDto?> UpdateAsync(
        string id,
        UpdateFieldActivityDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return null;
        }

        if (dto.Name != null) existing.Name = dto.Name.Trim();
        if (dto.Location != null) existing.Location = dto.Location.Trim();
        if (dto.Scheduled != null) existing.Scheduled = FormatScheduledDate(dto.Scheduled);
        if (dto.Team != null) existing.Team = dto.Team.Trim();
        if (dto.Target != null) existing.Target = dto.Target.Trim();
        if (dto.Sector != null) existing.Sector = dto.Sector.Trim();
        if (dto.Status != null) existing.Status = dto.Status.Trim();
        if (dto.Description != null) existing.Description = dto.Description.Trim();
        if (dto.BeneficiariesReached.HasValue) existing.BeneficiariesReached = dto.BeneficiariesReached.Value;

        existing.UpdatedAt = DateTimeOffset.UtcNow;
        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<FieldActivityDto?> UpdateStatusAsync(
        string id,
        UpdateFieldActivityStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await _repository.UpdateStatusAsync(id, dto.Status, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<FieldActivityMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var m = await _repository.GetMetricsAsync(cancellationToken);
        return new FieldActivityMetricsDto
        {
            ActiveToday = m.ActiveToday,
            PeopleTargeted = m.PeopleTargeted.ToString("N0", CultureInfo.InvariantCulture),
            PeopleReachedToday = m.PeopleReachedToday.ToString("N0", CultureInfo.InvariantCulture),
            TeamsDeployed = m.TeamsDeployed,
            StaffInField = m.StaffInField,
            UpdatesOverdue = m.UpdatesOverdue,
            TotalActivities = m.TotalActivities,
            NeedsActionCount = m.NeedsActionCount,
            ActiveCount = m.ActiveCount,
            CompletedCount = m.CompletedCount,
            ArchivedCount = m.ArchivedCount,
            LastUpdated = m.LastUpdated
        };
    }

    public async Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetToDefaultAsync(cancellationToken);
    }

    private static FieldActivityDto MapToDto(FieldActivity entity)
    {
        return new FieldActivityDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Location = entity.Location,
            Scheduled = entity.Scheduled,
            Team = entity.Team,
            Target = entity.Target,
            Sector = entity.Sector,
            Status = entity.Status,
            Description = entity.Description,
            BeneficiariesReached = entity.BeneficiariesReached,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CompletedAt = entity.CompletedAt
        };
    }

    private static string FormatScheduledDate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "Not scheduled";
        if (DateTimeOffset.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
        {
            return dt.ToString("dd MMM");
        }
        return input.Trim();
    }
}
