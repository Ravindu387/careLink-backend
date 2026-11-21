using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;

namespace CareLink.Application.Services;

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _repository;
    private static int _itemCounter = 8;
    private static int _requestCounter = 43;
    private static readonly object _counterLock = new();

    public ResourceService(IResourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ResourceDto>> GetAllAsync(
        ResourceQueryParameters? query = null,
        CancellationToken cancellationToken = default)
    {
        var filter = query == null
            ? null
            : new ResourceFilterParams
            {
                Search = query.Search,
                Area = query.Area,
                Status = query.Status,
                Availability = query.Availability,
                Category = query.Category,
            };

        var items = await _repository.GetAllAsync(filter, cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<ResourceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item == null ? null : MapToDto(item);
    }

    public async Task<ResourceDto> CreateItemAsync(
        CreateResourceItemDto dto,
        CancellationToken cancellationToken = default)
    {
        int id;
        lock (_counterLock)
        {
            _itemCounter++;
            id = _itemCounter;
        }

        var total = dto.Available + dto.Reserved;
        var availability = total > 0
            ? (int)Math.Round((double)dto.Available / total * 100)
            : 100;

        var status = availability > 60
            ? "Healthy"
            : availability >= 30
            ? "Low Stock"
            : "Critical";

        var item = new ResourceItem
        {
            Id = id,
            Name = dto.Name.Trim(),
            Category = dto.Category.Trim(),
            Location = dto.Location.Trim(),
            Available = dto.Available,
            Reserved = dto.Reserved,
            Availability = availability,
            Status = status,
            Unit = dto.Unit?.Trim() ?? "Units",
            LastRestocked = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var created = await _repository.AddAsync(item, cancellationToken);
        return MapToDto(created);
    }

    public async Task<ResourceDto?> UpdateStockAsync(
        int id,
        UpdateResourceStockDto dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await _repository.UpdateStockAsync(id, dto.Available, dto.Reserved, dto.Status, cancellationToken);
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<ResourceMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var m = await _repository.GetMetricsAsync(cancellationToken);
        return new ResourceMetricsDto
        {
            AvailabilityPercent = m.AverageAvailabilityPercent,
            TotalWarehouses = m.TotalWarehousesCount,
            CriticalShortages = m.CriticalShortagesCount,
            CriticalShortagesSummary = m.CriticalShortagesSummary,
            ActiveDispatches = m.ActiveDispatchesCount,
            ArrivingToday = m.ArrivingTodayDispatchesCount,
            PendingRequests = m.PendingRequestsCount,
            HighPriorityRequests = m.HighPriorityPendingRequestsCount,
            LastUpdated = m.LastUpdated,
        };
    }

    public async Task<ResourceRequestResponseDto> CreateRequestAsync(
        CreateResourceRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        string reqId;
        lock (_counterLock)
        {
            _requestCounter++;
            reqId = $"REQ-RES-{_requestCounter:D3}";
        }

        var req = new ResourceRequest
        {
            Id = reqId,
            ResourceName = dto.ResourceName.Trim(),
            Quantity = dto.Quantity,
            OperationalArea = dto.OperationalArea.Trim(),
            Priority = dto.Priority ?? "Normal",
            Status = "Pending",
            RequestedBy = "Emergency Coordinator",
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var created = await _repository.CreateRequestAsync(req, cancellationToken);
        return new ResourceRequestResponseDto
        {
            Id = created.Id,
            ResourceName = created.ResourceName,
            Quantity = created.Quantity,
            OperationalArea = created.OperationalArea,
            Priority = created.Priority,
            Status = created.Status,
            RequestedBy = created.RequestedBy,
            Notes = created.Notes,
            CreatedAt = created.CreatedAt,
            FulfilledAt = created.FulfilledAt,
        };
    }

    public async Task<IReadOnlyList<ResourceRequestResponseDto>> GetRequestsAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _repository.GetRequestsAsync(cancellationToken);
        return requests.Select(r => new ResourceRequestResponseDto
        {
            Id = r.Id,
            ResourceName = r.ResourceName,
            Quantity = r.Quantity,
            OperationalArea = r.OperationalArea,
            Priority = r.Priority,
            Status = r.Status,
            RequestedBy = r.RequestedBy,
            Notes = r.Notes,
            CreatedAt = r.CreatedAt,
            FulfilledAt = r.FulfilledAt,
        }).ToList();
    }

    public async Task ResetDefaultDataAsync(CancellationToken cancellationToken = default)
    {
        await _repository.ResetToDefaultAsync(cancellationToken);
    }

    private static ResourceDto MapToDto(ResourceItem r)
    {
        return new ResourceDto
        {
            Id = r.Id,
            Name = r.Name,
            Category = r.Category,
            Location = r.Location,
            Available = r.Available,
            Reserved = r.Reserved,
            Availability = r.Availability,
            Status = r.Status,
            Unit = r.Unit,
            LastRestocked = r.LastRestocked,
            UpdatedAt = r.UpdatedAt,
        };
    }
}
