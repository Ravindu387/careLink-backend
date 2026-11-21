using System.Globalization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class ResourceContributionService : IResourceContributionService
{
    private readonly IResourceContributionRepository _repository;

    public ResourceContributionService(IResourceContributionRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<ResourceContributionDto>> GetPagedAsync(
        ResourceContributionQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(parameters, cancellationToken);

        return new PagedResultDto<ResourceContributionDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<IEnumerable<ResourceContributionDto>> GetAllAsync(
        ResourceContributionQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(parameters, cancellationToken);
        return items.Select(MapToDto);
    }

    public async Task<ResourceContributionDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<ResourceContributionDto> CreateAsync(
        CreateResourceContributionDto dto,
        CancellationToken cancellationToken = default)
    {
        var id = !string.IsNullOrWhiteSpace(dto.Id)
            ? dto.Id.Trim()
            : $"DON-{new Random().Next(10000, 99999)}";

        DateTime receivedDate = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.ReceivedDate) &&
            DateTime.TryParse(dto.ReceivedDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            receivedDate = parsedDate;
        }

        var receivedString = !string.IsNullOrWhiteSpace(dto.Received)
            ? dto.Received.Trim()
            : receivedDate.ToString("dd MMM", CultureInfo.InvariantCulture);

        var entity = new ResourceContribution
        {
            Id = id,
            Donor = dto.Donor.Trim(),
            Type = string.IsNullOrWhiteSpace(dto.Type) ? "Material" : dto.Type.Trim(),
            Quantity = dto.Quantity.Trim(),
            Campaign = string.IsNullOrWhiteSpace(dto.Campaign) ? "Flood Response 2025" : dto.Campaign.Trim(),
            ReceivedDate = receivedDate,
            Received = receivedString,
            Verification = "Pending",
            Allocation = "Unallocated",
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<ResourceContributionDto?> UpdateAsync(
        string id,
        UpdateResourceContributionDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Donor)) existing.Donor = dto.Donor.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Type)) existing.Type = dto.Type.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Quantity)) existing.Quantity = dto.Quantity.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Campaign)) existing.Campaign = dto.Campaign.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Verification)) existing.Verification = dto.Verification.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Allocation)) existing.Allocation = dto.Allocation.Trim();
        if (dto.Notes != null) existing.Notes = dto.Notes.Trim();

        existing.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(existing, cancellationToken);
        return MapToDto(existing);
    }

    public async Task<bool> UpdateVerificationAsync(string id, string verification, CancellationToken cancellationToken = default)
    {
        return await _repository.UpdateVerificationAsync(id, verification, cancellationToken);
    }

    public async Task<bool> UpdateAllocationAsync(string id, string allocation, CancellationToken cancellationToken = default)
    {
        return await _repository.UpdateAllocationAsync(id, allocation, cancellationToken);
    }

    public async Task<int> BulkVerifyAsync(List<string> ids, CancellationToken cancellationToken = default)
    {
        return await _repository.BulkVerifyAsync(ids, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    private static ResourceContributionDto MapToDto(ResourceContribution entity)
    {
        return new ResourceContributionDto
        {
            Id = entity.Id,
            Donor = entity.Donor,
            Type = entity.Type,
            Quantity = entity.Quantity,
            Campaign = entity.Campaign,
            Received = entity.Received,
            ReceivedDate = entity.ReceivedDate,
            Verification = entity.Verification,
            Allocation = entity.Allocation,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
