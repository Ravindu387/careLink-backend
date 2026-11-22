using System.Globalization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class CollaborationRequestService : ICollaborationRequestService
{
    private readonly ICollaborationRequestRepository _repository;
    private static int _idCounter = 745;
    private static readonly object _lock = new();

    public CollaborationRequestService(ICollaborationRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CollaborationRequestDto>> GetAllAsync(
        CollaborationQueryParameters? query = null,
        CancellationToken cancellationToken = default)
    {
        var filter = query == null ? null : new CollaborationFilterParams
        {
            Search = query.Search,
            Tab = query.Tab,
            Sector = query.Sector,
            Partner = query.Partner,
            Area = query.Area,
            Status = query.Status
        };

        var items = await _repository.GetAllAsync(filter, cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<CollaborationRequestDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item == null ? null : MapToDto(item);
    }

    public async Task<CollaborationRequestDto> CreateAsync(
        CreateCollaborationRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        string id;
        lock (_lock)
        {
            _idCounter++;
            id = $"COL-0{_idCounter}";
        }

        var now = DateTimeOffset.UtcNow;
        var formattedStart = FormatStartDate(dto.StartDate);

        var entity = new CollaborationRequest
        {
            Id = id,
            Title = dto.Title.Trim(),
            Partner = dto.Partner.Trim(),
            Sector = string.IsNullOrWhiteSpace(dto.Sector) ? "WASH" : dto.Sector.Trim(),
            Area = string.IsNullOrWhiteSpace(dto.Area) ? "North Ward" : dto.Area.Trim(),
            Support = dto.Support.Trim(),
            StartDate = formattedStart,
            Status = "Awaiting Response",
            Description = dto.Description?.Trim(),
            ContactName = dto.ContactName?.Trim(),
            ContactEmail = dto.ContactEmail?.Trim(),
            ContactPhone = dto.ContactPhone?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<CollaborationRequestDto?> UpdateAsync(
        string id,
        UpdateCollaborationRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return null;
        }

        if (dto.Title != null) existing.Title = dto.Title.Trim();
        if (dto.Partner != null) existing.Partner = dto.Partner.Trim();
        if (dto.Sector != null) existing.Sector = dto.Sector.Trim();
        if (dto.Area != null) existing.Area = dto.Area.Trim();
        if (dto.Support != null) existing.Support = dto.Support.Trim();
        if (dto.StartDate != null) existing.StartDate = FormatStartDate(dto.StartDate);
        if (dto.Status != null) existing.Status = dto.Status.Trim();
        if (dto.Description != null) existing.Description = dto.Description.Trim();
        if (dto.ContactName != null) existing.ContactName = dto.ContactName.Trim();
        if (dto.ContactEmail != null) existing.ContactEmail = dto.ContactEmail.Trim();
        if (dto.ContactPhone != null) existing.ContactPhone = dto.ContactPhone.Trim();

        existing.UpdatedAt = DateTimeOffset.UtcNow;
        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<CollaborationRequestDto?> UpdateStatusAsync(
        string id,
        UpdateCollaborationStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await _repository.UpdateStatusAsync(id, dto.Status, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<CollaborationRequestMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var m = await _repository.GetMetricsAsync(cancellationToken);
        return new CollaborationRequestMetricsDto
        {
            OpenCollaborations = m.OpenCollaborations,
            AwaitingResponse = m.AwaitingResponse,
            ActivePartnerships = m.ActivePartnerships,
            CompletedThisMonth = m.CompletedThisMonth,
            TotalCollaborations = m.TotalCollaborations,
            SectorsCount = m.SectorsCount,
            PartnersCount = m.PartnersCount,
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

    private static CollaborationRequestDto MapToDto(CollaborationRequest entity)
    {
        return new CollaborationRequestDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Partner = entity.Partner,
            Sector = entity.Sector,
            Area = entity.Area,
            Support = entity.Support,
            StartDate = entity.StartDate,
            Status = entity.Status,
            Description = entity.Description,
            ContactName = entity.ContactName,
            ContactEmail = entity.ContactEmail,
            ContactPhone = entity.ContactPhone,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            AcceptedAt = entity.AcceptedAt,
            CompletedAt = entity.CompletedAt
        };
    }

    private static string FormatStartDate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "Not scheduled";
        if (DateTimeOffset.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
        {
            return dt.ToString("dd MMM");
        }
        return input.Trim();
    }
}
