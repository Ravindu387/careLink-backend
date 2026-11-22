using System.Globalization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class DonorService : IDonorService
{
    private readonly IDonorRepository _donorRepository;

    public DonorService(IDonorRepository donorRepository)
    {
        _donorRepository = donorRepository;
    }

    public async Task<IEnumerable<DonorDto>> GetAllDonorsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _donorRepository.GetAllAsync(cancellationToken);
        return items.Select(MapToDto);
    }

    public async Task<PagedResultDto<DonorDto>> GetPagedDonorsAsync(DonorQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _donorRepository.GetPagedAsync(parameters, cancellationToken);

        var page = parameters.Page.GetValueOrDefault(1);
        if (page < 1) page = 1;
        var pageSize = parameters.PageSize.GetValueOrDefault(10);
        if (pageSize < 1) pageSize = 10;

        return new PagedResultDto<DonorDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<DonorDto?> GetDonorByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await _donorRepository.GetByIdAsync(id, cancellationToken);
        if (item == null) return null;
        return MapToDto(item);
    }

    public async Task<DonorDto> CreateDonorAsync(CreateDonorDto dto, CancellationToken cancellationToken = default)
    {
        var all = await _donorRepository.GetAllAsync(cancellationToken);
        var existingIds = all.Select(d => d.Id.ToUpperInvariant()).ToHashSet();

        // Calculate next ID like DNR-2050
        var maxNum = all
            .Select(d => d.Id.Replace("DNR-", "", StringComparison.OrdinalIgnoreCase))
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .DefaultIfEmpty(2000)
            .Max();

        var id = $"DNR-{(maxNum + 1)}";
        while (existingIds.Contains(id.ToUpperInvariant()))
        {
            maxNum++;
            id = $"DNR-{(maxNum + 1)}";
        }

        var donor = new Donor
        {
            Id = id,
            Name = dto.Name.Trim(),
            Type = dto.Type.Trim(),
            Contact = dto.Contact.Trim(),
            Scope = dto.Scope.Trim(),
            Verification = string.IsNullOrWhiteSpace(dto.Verification) ? "Pending" : dto.Verification.Trim(),
            Contributions = dto.Contributions,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim(),
            Address = dto.Address?.Trim(),
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _donorRepository.AddAsync(donor, cancellationToken);
        return MapToDto(created);
    }

    public async Task<DonorDto?> UpdateDonorAsync(string id, UpdateDonorDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _donorRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        existing.Name = dto.Name.Trim();
        existing.Type = dto.Type.Trim();
        existing.Contact = dto.Contact.Trim();
        existing.Scope = dto.Scope.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Verification))
        {
            existing.Verification = dto.Verification.Trim();
        }
        if (dto.Contributions.HasValue)
        {
            existing.Contributions = dto.Contributions.Value;
        }
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            existing.Status = dto.Status.Trim();
        }
        existing.Address = dto.Address?.Trim();
        existing.Notes = dto.Notes?.Trim();
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _donorRepository.UpdateAsync(existing, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<DonorDto?> UpdateDonorStatusAsync(string id, string status, CancellationToken cancellationToken = default)
    {
        var existing = await _donorRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        existing.Status = status.Trim();
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _donorRepository.UpdateAsync(existing, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<DonorDto?> UpdateDonorVerificationAsync(string id, string verification, CancellationToken cancellationToken = default)
    {
        var existing = await _donorRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        existing.Verification = verification.Trim();
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _donorRepository.UpdateAsync(existing, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<int> BulkActionAsync(BulkDonorActionDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Ids == null || !dto.Ids.Any()) return 0;

        var action = dto.Action.Trim().ToLower();
        string newStatus;
        if (action == "activate")
        {
            newStatus = "Active";
        }
        else if (action == "restrict")
        {
            newStatus = "Restricted";
        }
        else if (action == "deactivate" || action == "inactive")
        {
            newStatus = "Inactive";
        }
        else
        {
            return 0;
        }

        return await _donorRepository.BulkUpdateStatusAsync(dto.Ids, newStatus, cancellationToken);
    }

    public async Task<bool> DeleteDonorAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _donorRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<DonorSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var metrics = await _donorRepository.GetSummaryMetricsAsync(cancellationToken);
        return new DonorSummaryDto
        {
            TotalDonors = metrics.TotalDonors,
            ActiveDonors = metrics.ActiveDonors,
            VerifiedDonors = metrics.VerifiedDonors,
            UnderReviewDonors = metrics.UnderReviewDonors,
            RestrictedDonors = metrics.RestrictedDonors,
            TotalContributions = metrics.TotalContributions
        };
    }

    private static DonorDto MapToDto(Donor entity)
    {
        return new DonorDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.Type,
            Contact = entity.Contact,
            Scope = entity.Scope,
            Verification = entity.Verification,
            Contributions = entity.Contributions,
            Status = entity.Status,
            Address = entity.Address,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt.ToString("o"),
            UpdatedAt = entity.UpdatedAt?.ToString("o")
        };
    }
}
