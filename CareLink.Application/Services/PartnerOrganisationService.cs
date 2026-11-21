using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class PartnerOrganisationService : IPartnerOrganisationService
{
    private readonly IPartnerOrganisationRepository _repository;
    private static int _orgCounter = 48;
    private static readonly object _counterLock = new();

    public PartnerOrganisationService(IPartnerOrganisationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PartnerOrganisationDto>> GetAllAsync(
        PartnerOrganisationQueryParameters? query = null,
        CancellationToken cancellationToken = default)
    {
        var filterParams = query == null
            ? null
            : new PartnerOrganisationFilterParams
            {
                Search = query.Search,
                Region = query.Region,
                Status = query.Status,
                Verification = query.Verification,
                Type = query.Type,
                Role = query.Role,
            };

        var list = await _repository.GetAllAsync(filterParams, cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<PartnerOrganisationDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<PartnerOrganisationDto> CreateAsync(
        CreatePartnerOrganisationDto dto,
        CancellationToken cancellationToken = default)
    {
        string id;
        lock (_counterLock)
        {
            _orgCounter++;
            id = $"ORG-{_orgCounter:D3}";
        }

        var entity = new PartnerOrganisation
        {
            Id = id,
            Name = dto.Name.Trim(),
            Type = dto.Type.Trim(),
            Region = dto.Region.Trim(),
            Role = dto.Role.Trim(),
            Verification = "Pending",
            Status = "Active",
            Email = dto.Email.Trim(),
            Phone = dto.Phone.Trim(),
            ContactPerson = dto.ContactPerson?.Trim(),
            SharedResourcesCount = 0,
            CollaborationRequestsCount = 0,
            Address = dto.Address?.Trim(),
            Website = dto.Website?.Trim(),
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            VerifiedAt = null,
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<PartnerOrganisationDto?> UpdateAsync(
        string id,
        UpdatePartnerOrganisationDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
            existing.Name = dto.Name.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Type))
            existing.Type = dto.Type.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Region))
            existing.Region = dto.Region.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Role))
            existing.Role = dto.Role.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Email))
            existing.Email = dto.Email.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Phone))
            existing.Phone = dto.Phone.Trim();

        if (dto.ContactPerson != null)
            existing.ContactPerson = dto.ContactPerson.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Status))
            existing.Status = dto.Status.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Verification))
        {
            existing.Verification = dto.Verification.Trim();
            if (existing.Verification.Equals("Verified", StringComparison.OrdinalIgnoreCase))
            {
                existing.VerifiedAt ??= DateTimeOffset.UtcNow;
            }
        }

        if (dto.Address != null)
            existing.Address = dto.Address.Trim();

        if (dto.Website != null)
            existing.Website = dto.Website.Trim();

        if (dto.Notes != null)
            existing.Notes = dto.Notes.Trim();

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<PartnerOrganisationDto?> UpdateStatusAsync(
        string id,
        UpdatePartnerStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await _repository.UpdateStatusAsync(id, dto.Status, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<PartnerOrganisationDto?> UpdateVerificationAsync(
        string id,
        UpdatePartnerVerificationDto dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await _repository.UpdateVerificationAsync(id, dto.Verification, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<PartnerOrganisationsMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var m = await _repository.GetMetricsAsync(cancellationToken);
        return new PartnerOrganisationsMetricsDto
        {
            ActivePartnersCount = m.ActivePartnersCount,
            TotalPartnersCount = m.TotalPartnersCount,
            SharedResourcesCount = m.SharedResourcesCount,
            CollaborationRequestsCount = m.CollaborationRequestsCount,
            PendingCollaborationRequestsCount = m.PendingCollaborationRequestsCount,
            VerificationCoveragePercent = m.VerificationCoveragePercent,
            PendingVerificationReviewsCount = m.PendingVerificationReviewsCount,
            SectorsCount = m.SectorsCount,
            LastUpdated = m.LastUpdated,
        };
    }

    public async Task ResetDefaultDataAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetToDefaultAsync(cancellationToken);
    }

    private static PartnerOrganisationDto MapToDto(PartnerOrganisation e)
    {
        return new PartnerOrganisationDto
        {
            Id = e.Id,
            Name = e.Name,
            Type = e.Type,
            Region = e.Region,
            Role = e.Role,
            Verification = e.Verification,
            Status = e.Status,
            Email = e.Email,
            Phone = e.Phone,
            ContactPerson = e.ContactPerson,
            SharedResourcesCount = e.SharedResourcesCount,
            CollaborationRequestsCount = e.CollaborationRequestsCount,
            Address = e.Address,
            Website = e.Website,
            Notes = e.Notes,
            CreatedAt = e.CreatedAt,
            VerifiedAt = e.VerifiedAt,
        };
    }
}
