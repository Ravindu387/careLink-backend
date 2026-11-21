using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class PartnerDocumentService : IPartnerDocumentService
{
    private readonly IPartnerDocumentRepository _repository;
    private static int _idCounter = 415;
    private static readonly object _lock = new();

    public PartnerDocumentService(IPartnerDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PartnerDocumentDto>> GetAllAsync(
        PartnerDocumentQueryParameters? query = null,
        CancellationToken cancellationToken = default)
    {
        var filter = query == null ? null : new DocumentFilterParams
        {
            Search = query.Search,
            Tab = query.Tab,
            Type = query.Type,
            Owner = query.Owner,
            Visibility = query.Visibility,
            Status = query.Status
        };

        var items = await _repository.GetAllAsync(filter, cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<PartnerDocumentDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item == null ? null : MapToDto(item);
    }

    public async Task<PartnerDocumentDto> CreateAsync(
        CreatePartnerDocumentDto dto,
        CancellationToken cancellationToken = default)
    {
        string id;
        lock (_lock)
        {
            _idCounter++;
            id = $"DOC-0{_idCounter}";
        }

        var now = DateTimeOffset.UtcNow;
        var updatedStr = now.ToString("dd MMM - HH:mm");

        var entity = new PartnerDocument
        {
            Id = id,
            Name = dto.Name.Trim(),
            Type = string.IsNullOrWhiteSpace(dto.Type) ? "Operational Plan" : dto.Type.Trim(),
            Owner = string.IsNullOrWhiteSpace(dto.Owner) ? "Sara Khan" : dto.Owner.Trim(),
            Updated = updatedStr,
            Version = string.IsNullOrWhiteSpace(dto.Version) ? "v1.0" : dto.Version.Trim(),
            Visibility = string.IsNullOrWhiteSpace(dto.Visibility) ? "Internal" : dto.Visibility.Trim(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Under Review" : dto.Status.Trim(),
            FileName = dto.FileName?.Trim(),
            FileUrl = dto.FileUrl?.Trim(),
            FileSizeBytes = dto.FileSizeBytes,
            Description = dto.Description?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<PartnerDocumentDto?> UpdateAsync(
        string id,
        UpdatePartnerDocumentDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return null;
        }

        if (dto.Name != null) existing.Name = dto.Name.Trim();
        if (dto.Type != null) existing.Type = dto.Type.Trim();
        if (dto.Owner != null) existing.Owner = dto.Owner.Trim();
        if (dto.Visibility != null) existing.Visibility = dto.Visibility.Trim();
        if (dto.Status != null) existing.Status = dto.Status.Trim();
        if (dto.Version != null) existing.Version = dto.Version.Trim();
        if (dto.FileName != null) existing.FileName = dto.FileName.Trim();
        if (dto.FileUrl != null) existing.FileUrl = dto.FileUrl.Trim();
        if (dto.Description != null) existing.Description = dto.Description.Trim();

        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.Updated = existing.UpdatedAt.ToString("dd MMM - HH:mm");

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<PartnerDocumentDto?> UpdateStatusAsync(
        string id,
        UpdatePartnerDocumentStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await _repository.UpdateStatusAsync(id, dto.Status, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<PartnerDocumentMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var m = await _repository.GetMetricsAsync(cancellationToken);
        return new PartnerDocumentMetricsDto
        {
            ActiveRecords = m.ActiveRecords,
            NeedsReview = m.NeedsReview,
            VerifiedPercentage = m.VerifiedPercentage,
            DueSoon = m.DueSoon,
            TotalDocuments = m.TotalDocuments,
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

    private static PartnerDocumentDto MapToDto(PartnerDocument entity)
    {
        return new PartnerDocumentDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.Type,
            Owner = entity.Owner,
            Updated = entity.Updated,
            Version = entity.Version,
            Visibility = entity.Visibility,
            Status = entity.Status,
            FileName = entity.FileName,
            FileUrl = entity.FileUrl,
            FileSizeBytes = entity.FileSizeBytes,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
