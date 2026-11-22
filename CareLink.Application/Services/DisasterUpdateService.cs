using System.Globalization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class DisasterUpdateService : IDisasterUpdateService
{
    private readonly IDisasterUpdateRepository _repository;

    public DisasterUpdateService(IDisasterUpdateRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<DisasterUpdateDto>> GetUpdatesAsync(
        DisasterUpdateQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(parameters, cancellationToken);
        return items.Select(MapToDto);
    }

    public async Task<DisasterUpdateDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<DisasterUpdateSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        // Get all updates to calculate aggregate summary
        var all = await _repository.GetAllAsync(new DisasterUpdateQueryParameters(), cancellationToken);
        var allList = all.ToList();

        var active = allList.Where(x => !x.Archived && !x.IsDraft).ToList();
        var drafts = allList.Where(x => x.IsDraft).ToList();
        var archived = allList.Where(x => x.Archived && !x.IsDraft).ToList();

        var latestCritical = active
            .Where(x => x.Type.Equals("Critical", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        return new DisasterUpdateSummaryDto
        {
            TotalActive = active.Count,
            CriticalCount = active.Count(x => x.Type.Equals("Critical", StringComparison.OrdinalIgnoreCase)),
            WarningCount = active.Count(x => x.Type.Equals("Warning", StringComparison.OrdinalIgnoreCase)),
            AnnouncementCount = active.Count(x => x.Type.Equals("Announcement", StringComparison.OrdinalIgnoreCase)),
            UpdateCount = active.Count(x => x.Type.Equals("Update", StringComparison.OrdinalIgnoreCase)),
            ArchivedCount = archived.Count,
            DraftCount = drafts.Count,
            LatestCriticalAlert = latestCritical != null ? MapToDto(latestCritical) : null
        };
    }

    public async Task<DisasterUpdateDto> CreateAsync(
        CreateDisasterUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        string id;

        if (dto.IsDraft)
        {
            id = $"DRAFT-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }
        else
        {
            var all = await _repository.GetAllAsync(new DisasterUpdateQueryParameters(), cancellationToken);
            var maxId = 100;
            foreach (var item in all)
            {
                if (item.Id.StartsWith("DU-") && int.TryParse(item.Id.Substring(3), out var n))
                {
                    if (n > maxId) maxId = n;
                }
            }
            id = $"DU-{maxId + 1}";
        }

        var entity = new DisasterUpdate
        {
            Id = id,
            Type = string.IsNullOrWhiteSpace(dto.Type) ? "Announcement" : dto.Type.Trim(),
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Source = string.IsNullOrWhiteSpace(dto.Source) ? "CareLink Coordination" : dto.Source.Trim(),
            Location = string.IsNullOrWhiteSpace(dto.Location) ? "Greater Riverton" : dto.Location.Trim(),
            PublishedBy = string.IsNullOrWhiteSpace(dto.PublishedBy) ? "Sanjana A." : dto.PublishedBy.Trim(),
            Instructions = string.IsNullOrWhiteSpace(dto.Instructions) ? dto.Description.Trim() : dto.Instructions.Trim(),
            Archived = false,
            IsDraft = dto.IsDraft,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<DisasterUpdateDto?> ToggleArchiveAsync(string id, CancellationToken cancellationToken = default)
    {
        var success = await _repository.ToggleArchiveAsync(id, cancellationToken);
        if (!success) return null;

        var updated = await _repository.GetByIdAsync(id, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<DisasterUpdateDto?> PublishDraftAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null || !existing.IsDraft) return null;

        var all = await _repository.GetAllAsync(new DisasterUpdateQueryParameters(), cancellationToken);
        var maxId = 100;
        foreach (var item in all)
        {
            if (item.Id.StartsWith("DU-") && int.TryParse(item.Id.Substring(3), out var n))
            {
                if (n > maxId) maxId = n;
            }
        }

        // Delete old draft entry and create as published DU-XXX, or update
        await _repository.DeleteAsync(existing.Id, cancellationToken);

        existing.Id = $"DU-{maxId + 1}";
        existing.IsDraft = false;
        existing.CreatedAt = DateTime.UtcNow;
        existing.UpdatedAt = DateTime.UtcNow;

        var published = await _repository.AddAsync(existing, cancellationToken);
        return MapToDto(published);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<EmergencyContactDto>> GetEmergencyContactsAsync(CancellationToken cancellationToken = default)
    {
        var contacts = await _repository.GetEmergencyContactsAsync(cancellationToken);
        return contacts.Select(c => new EmergencyContactDto
        {
            Id = c.Id,
            Name = c.Name,
            Number = c.Number
        });
    }

    private static DisasterUpdateDto MapToDto(DisasterUpdate entity)
    {
        return new DisasterUpdateDto
        {
            Id = entity.Id,
            Type = entity.Type,
            Title = entity.Title,
            Description = entity.Description,
            Source = entity.Source,
            Date = entity.CreatedAt.ToString("dd MMM", CultureInfo.InvariantCulture),
            Time = entity.IsDraft ? "--:--" : entity.CreatedAt.ToString("HH:mm", CultureInfo.InvariantCulture),
            Location = entity.Location,
            PublishedBy = entity.PublishedBy,
            Instructions = entity.Instructions,
            Archived = entity.Archived,
            IsDraft = entity.IsDraft,
            CreatedAtUtc = entity.CreatedAt
        };
    }
}
