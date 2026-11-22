namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _repository;

    public WarehouseService(IWarehouseRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IEnumerable<WarehouseDto>>> GetWarehousesAsync(
        string? tab,
        string? search,
        string? district,
        string? capacityState,
        string? status)
    {
        var warehouses = await _repository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(tab))
        {
            var t = tab.Trim().ToLowerInvariant();
            warehouses = t switch
            {
                "needs action" or "needsaction" => warehouses.Where(w =>
                    w.Utilisation >= 85 ||
                    string.Equals(w.Status, "Under Maintenance", StringComparison.OrdinalIgnoreCase)),
                "active" => warehouses.Where(w =>
                    string.Equals(w.Status, "Operational", StringComparison.OrdinalIgnoreCase)),
                "completed" => warehouses.Where(w =>
                    string.Equals(w.Status, "Operational", StringComparison.OrdinalIgnoreCase)),
                "exceptions" => warehouses.Where(w =>
                    string.Equals(w.Status, "Under Maintenance", StringComparison.OrdinalIgnoreCase)),
                _ => warehouses
            };
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            warehouses = warehouses.Where(w =>
                (w.Id?.ToLowerInvariant().Contains(s) ?? false) ||
                (w.Name?.ToLowerInvariant().Contains(s) ?? false) ||
                (w.Location?.ToLowerInvariant().Contains(s) ?? false) ||
                (w.Status?.ToLowerInvariant().Contains(s) ?? false)
            );
        }

        if (!string.IsNullOrWhiteSpace(district) && !district.Equals("All districts", StringComparison.OrdinalIgnoreCase))
        {
            warehouses = warehouses.Where(w => string.Equals(w.Location, district, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(capacityState) && !capacityState.Equals("All capacity states", StringComparison.OrdinalIgnoreCase))
        {
            if (capacityState.Equals("Normal", StringComparison.OrdinalIgnoreCase))
            {
                warehouses = warehouses.Where(w => w.Utilisation < 85);
            }
            else if (capacityState.Equals("Near Capacity", StringComparison.OrdinalIgnoreCase))
            {
                warehouses = warehouses.Where(w => w.Utilisation >= 85);
            }
        }

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            warehouses = warehouses.Where(w => string.Equals(w.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        var dtos = warehouses.Select(MapToDto);
        return ApiResponse<IEnumerable<WarehouseDto>>.SuccessResponse(dtos, "Warehouses retrieved successfully");
    }

    public async Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(string id)
    {
        var warehouse = await _repository.GetByIdAsync(id);
        if (warehouse == null)
        {
            return ApiResponse<WarehouseDto>.ErrorResponse($"Warehouse with id '{id}' not found");
        }

        return ApiResponse<WarehouseDto>.SuccessResponse(MapToDto(warehouse), "Warehouse retrieved successfully");
    }

    public async Task<ApiResponse<WarehouseSummaryDto>> GetSummaryAsync()
    {
        var all = (await _repository.GetAllAsync()).ToList();

        var summary = new WarehouseSummaryDto
        {
            TotalCount = all.Count,
            OperationalSites = all.Count(w => string.Equals(w.Status, "Operational", StringComparison.OrdinalIgnoreCase)),
            AverageUtilisation = all.Count > 0 ? Math.Round(all.Average(w => w.Utilisation), 1) : 0,
            ReceivingToday = 8,
            CapacityAlerts = all.Count(w => w.Utilisation >= 85 || string.Equals(w.Status, "Near Capacity", StringComparison.OrdinalIgnoreCase))
        };

        return ApiResponse<WarehouseSummaryDto>.SuccessResponse(summary, "Warehouse summary retrieved successfully");
    }

    public async Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiResponse<WarehouseDto>.ErrorResponse("Warehouse name is required");
        }

        if (string.IsNullOrWhiteSpace(request.Location))
        {
            return ApiResponse<WarehouseDto>.ErrorResponse("Location / district is required");
        }

        if (request.Capacity <= 0)
        {
            return ApiResponse<WarehouseDto>.ErrorResponse("Capacity must be greater than zero");
        }

        var entity = new Warehouse
        {
            Name = request.Name.Trim(),
            Location = request.Location.Trim(),
            Capacity = request.Capacity,
            Staff = request.Staff,
            Utilisation = request.Utilisation ?? 0,
            StockLines = request.StockLines ?? 0,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Operational" : request.Status.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(entity);
        return ApiResponse<WarehouseDto>.SuccessResponse(MapToDto(created), "Warehouse created successfully");
    }

    public async Task<ApiResponse<WarehouseDto>> UpdateStatusAsync(string id, UpdateWarehouseStatusRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<WarehouseDto>.ErrorResponse($"Warehouse with id '{id}' not found");
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var validStatuses = new[] { "Operational", "Near Capacity", "Under Maintenance" };
            var matched = validStatuses.FirstOrDefault(s => string.Equals(s, request.Status.Trim(), StringComparison.OrdinalIgnoreCase));
            if (matched != null)
            {
                existing.Status = matched;
            }
        }

        if (request.Utilisation.HasValue)
        {
            existing.Utilisation = Math.Clamp(request.Utilisation.Value, 0, 100);
            if (existing.Utilisation >= 85 && existing.Status == "Operational")
            {
                existing.Status = "Near Capacity";
            }
        }

        if (request.Staff.HasValue && request.Staff.Value >= 0)
        {
            existing.Staff = request.Staff.Value;
        }

        if (request.StockLines.HasValue && request.StockLines.Value >= 0)
        {
            existing.StockLines = request.StockLines.Value;
        }

        var updated = await _repository.UpdateAsync(existing);
        return ApiResponse<WarehouseDto>.SuccessResponse(MapToDto(updated!), "Warehouse updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteWarehouseAsync(string id)
    {
        var success = await _repository.DeleteAsync(id);
        if (!success)
        {
            return ApiResponse<bool>.ErrorResponse($"Warehouse with id '{id}' not found");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Warehouse deleted successfully");
    }

    private static WarehouseDto MapToDto(Warehouse entity)
    {
        return new WarehouseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Location = entity.Location,
            Capacity = entity.Capacity,
            Utilisation = entity.Utilisation,
            StockLines = entity.StockLines,
            Staff = entity.Staff,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
