using System.Globalization;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;

namespace CareLink.Application.Services;

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IShelterRepository _shelterRepository;

    public ResourceService(
        IResourceRepository resourceRepository,
        IShelterRepository shelterRepository)
    {
        _resourceRepository = resourceRepository;
        _shelterRepository = shelterRepository;
    }

    public async Task<PagedResultDto<ResourceDto>> GetPagedResourcesAsync(
        ResourceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _resourceRepository.GetPagedAsync(parameters, cancellationToken);

        return new PagedResultDto<ResourceDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<IEnumerable<ResourceDto>> GetAllResourcesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _resourceRepository.GetAllAsync(cancellationToken);
        return items.Select(MapToDto);
    }

    public async Task<ResourceDto?> GetResourceByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await _resourceRepository.GetByIdAsync(id, cancellationToken);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<ResourceSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var metrics = await _resourceRepository.GetSummaryMetricsAsync(cancellationToken);

        return new ResourceSummaryDto
        {
            HealthyCount = metrics.HealthyCount,
            LowStockCount = metrics.LowStockCount,
            CriticalCount = metrics.CriticalCount,
            OutOfStockCount = metrics.OutOfStockCount,
            ImmediateActionCount = metrics.ImmediateActionCount,
            ExpiringSoonCount = metrics.ExpiringSoonCount,
            IncomingDonationsCount = 4,
            TotalItemsCount = metrics.TotalItemsCount
        };
    }

    public async Task<ResourceDto> CreateResourceAsync(
        CreateResourceDto dto,
        CancellationToken cancellationToken = default)
    {
        string shelterId = dto.Shelter;
        string shelterName = dto.Shelter;

        var allShelters = await _shelterRepository.GetAllAsync(cancellationToken);
        var matchedShelter = allShelters.FirstOrDefault(s =>
            s.Id.Equals(dto.Shelter, StringComparison.OrdinalIgnoreCase) ||
            s.Name.Equals(dto.Shelter, StringComparison.OrdinalIgnoreCase));

        if (matchedShelter != null)
        {
            shelterId = matchedShelter.Id;
            shelterName = matchedShelter.Name;
        }

        DateTime? parsedExpiry = null;
        if (!string.IsNullOrWhiteSpace(dto.ExpiryDate) && dto.ExpiryDate != "—")
        {
            if (DateTime.TryParse(dto.ExpiryDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                parsedExpiry = dt;
            }
        }

        var randomNum = Random.Shared.Next(150, 999);
        var id = $"INV-{randomNum}";

        var entity = new ResourceItem
        {
            Id = id,
            Name = dto.Name.Trim(),
            Category = dto.Category.Trim(),
            Available = Math.Max(0, dto.Available),
            Minimum = Math.Max(1, dto.Minimum),
            Unit = string.IsNullOrWhiteSpace(dto.Unit) ? "Units" : dto.Unit.Trim(),
            ExpiryDate = parsedExpiry,
            LastUpdated = DateTime.UtcNow,
            ShelterId = shelterId,
            ShelterName = shelterName,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _resourceRepository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<ResourceDto?> UpdateResourceAsync(
        string id,
        UpdateResourceDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _resourceRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name)) existing.Name = dto.Name.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Category)) existing.Category = dto.Category.Trim();
        if (dto.Available.HasValue) existing.Available = Math.Max(0, dto.Available.Value);
        if (dto.Minimum.HasValue) existing.Minimum = Math.Max(1, dto.Minimum.Value);
        if (!string.IsNullOrWhiteSpace(dto.Unit)) existing.Unit = dto.Unit.Trim();

        if (dto.ExpiryDate != null)
        {
            if (string.IsNullOrWhiteSpace(dto.ExpiryDate) || dto.ExpiryDate == "—")
            {
                existing.ExpiryDate = null;
            }
            else if (DateTime.TryParse(dto.ExpiryDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                existing.ExpiryDate = dt;
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.Shelter))
        {
            var allShelters = await _shelterRepository.GetAllAsync(cancellationToken);
            var matched = allShelters.FirstOrDefault(s =>
                s.Id.Equals(dto.Shelter, StringComparison.OrdinalIgnoreCase) ||
                s.Name.Equals(dto.Shelter, StringComparison.OrdinalIgnoreCase));

            if (matched != null)
            {
                existing.ShelterId = matched.Id;
                existing.ShelterName = matched.Name;
            }
            else
            {
                existing.ShelterName = dto.Shelter.Trim();
            }
        }

        existing.LastUpdated = DateTime.UtcNow;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _resourceRepository.UpdateAsync(existing, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<ResourceDto?> RecordDistributionAsync(
        RecordDistributionDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _resourceRepository.GetByIdAsync(dto.ResourceId, cancellationToken);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Resource with ID '{dto.ResourceId}' not found.");
        }

        if (dto.Quantity <= 0)
        {
            throw new InvalidOperationException("Distribution quantity must be greater than zero.");
        }

        if (dto.Quantity > existing.Available)
        {
            throw new InvalidOperationException($"Insufficient stock for this distribution. Available: {existing.Available}, Requested: {dto.Quantity}.");
        }

        existing.Available -= dto.Quantity;
        existing.LastUpdated = DateTime.UtcNow;
        existing.UpdatedAt = DateTime.UtcNow;

        await _resourceRepository.UpdateAsync(existing, cancellationToken);

        // Record distribution log
        var distId = $"DIST-{Random.Shared.Next(1000, 9999)}";
        var distribution = new ResourceDistribution
        {
            Id = distId,
            ResourceId = existing.Id,
            ResourceName = existing.Name,
            Quantity = dto.Quantity,
            DestinationShelter = dto.Shelter.Trim(),
            DestinationShelterId = existing.ShelterId,
            Recipient = dto.Recipient.Trim(),
            Notes = dto.Notes?.Trim() ?? string.Empty,
            DistributedAt = DateTime.UtcNow
        };

        await _resourceRepository.AddDistributionAsync(distribution, cancellationToken);

        return MapToDto(existing);
    }

    public async Task<IEnumerable<ResourceDistributionDto>> GetDistributionsAsync(
        string? resourceId = null,
        CancellationToken cancellationToken = default)
    {
        var items = await _resourceRepository.GetDistributionsAsync(resourceId, cancellationToken);
        return items.Select(d => new ResourceDistributionDto
        {
            Id = d.Id,
            ResourceId = d.ResourceId,
            ResourceName = d.ResourceName,
            Quantity = d.Quantity,
            DestinationShelter = d.DestinationShelter,
            DestinationShelterId = d.DestinationShelterId,
            Recipient = d.Recipient,
            Notes = d.Notes,
            DistributedAt = d.DistributedAt.ToString("dd MMM · HH:mm", CultureInfo.InvariantCulture),
            DistributedAtUtc = d.DistributedAt
        });
    }

    public async Task<bool> DeleteResourceAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _resourceRepository.DeleteAsync(id, cancellationToken);
    }

    private static ResourceDto MapToDto(ResourceItem entity)
    {
        var status = ComputeStatus(entity.Available, entity.Minimum);
        var expiry = entity.ExpiryDate.HasValue
            ? entity.ExpiryDate.Value.ToString("dd MMM", CultureInfo.InvariantCulture)
            : "—";

        var isToday = entity.LastUpdated.Date == DateTime.UtcNow.Date;
        var lastUpdated = isToday
            ? "Today"
            : entity.LastUpdated.ToString("dd MMM", CultureInfo.InvariantCulture);

        return new ResourceDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Category = entity.Category,
            Available = entity.Available,
            Minimum = entity.Minimum,
            Unit = entity.Unit,
            Expiry = expiry,
            ExpiryDate = entity.ExpiryDate,
            LastUpdated = lastUpdated,
            LastUpdatedUtc = entity.LastUpdated,
            Shelter = entity.ShelterName,
            ShelterId = entity.ShelterId,
            Status = status
        };
    }

    private static string ComputeStatus(int available, int minimum)
    {
        if (available == 0) return "Out of Stock";
        if (available < minimum * 0.5) return "Critical";
        if (available < minimum) return "Low Stock";
        return "Healthy";
    }
}
