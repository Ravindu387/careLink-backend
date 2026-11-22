namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class AllocationService : IAllocationService
{
    private readonly IAllocationRepository _repository;

    public AllocationService(IAllocationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IEnumerable<AllocationDto>>> GetAllocationsAsync(
        string? tab,
        string? search,
        string? warehouse,
        string? destination,
        string? status)
    {
        var allocations = await _repository.GetAllAsync();

        // Filter by tab
        if (!string.IsNullOrWhiteSpace(tab) && !string.Equals(tab, "All", StringComparison.OrdinalIgnoreCase))
        {
            allocations = tab.Trim().ToLowerInvariant() switch
            {
                "needs action" or "needsaction" => allocations.Where(a =>
                    string.Equals(a.Status, "Pending", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(a.Status, "Partially Allocated", StringComparison.OrdinalIgnoreCase)),
                "active" => allocations.Where(a =>
                    string.Equals(a.Status, "Allocated", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(a.Status, "Released", StringComparison.OrdinalIgnoreCase)),
                "completed" => allocations.Where(a =>
                    string.Equals(a.Status, "Completed", StringComparison.OrdinalIgnoreCase)),
                "exceptions" => allocations.Where(a =>
                    string.Equals(a.Status, "Partially Allocated", StringComparison.OrdinalIgnoreCase)),
                _ => allocations
            };
        }

        // Filter by search keyword
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLowerInvariant();
            allocations = allocations.Where(a =>
                (a.Id?.ToLowerInvariant().Contains(keyword) ?? false) ||
                (a.Request?.ToLowerInvariant().Contains(keyword) ?? false) ||
                (a.Resource?.ToLowerInvariant().Contains(keyword) ?? false) ||
                (a.Destination?.ToLowerInvariant().Contains(keyword) ?? false) ||
                (a.Source?.ToLowerInvariant().Contains(keyword) ?? false)
            );
        }

        // Filter by warehouse
        if (!string.IsNullOrWhiteSpace(warehouse) && !warehouse.Equals("All warehouses", StringComparison.OrdinalIgnoreCase))
        {
            allocations = allocations.Where(a => string.Equals(a.Warehouse, warehouse, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by destination
        if (!string.IsNullOrWhiteSpace(destination) && !destination.Equals("All destinations", StringComparison.OrdinalIgnoreCase))
        {
            allocations = allocations.Where(a => string.Equals(a.Destination, destination, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All allocation states", StringComparison.OrdinalIgnoreCase))
        {
            allocations = allocations.Where(a => string.Equals(a.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        var dtos = allocations.Select(MapToDto);
        return ApiResponse<IEnumerable<AllocationDto>>.SuccessResponse(dtos, "Allocations retrieved successfully");
    }

    public async Task<ApiResponse<AllocationDto>> GetAllocationByIdAsync(string id)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item == null)
        {
            return ApiResponse<AllocationDto>.ErrorResponse($"Allocation with id '{id}' not found");
        }
        return ApiResponse<AllocationDto>.SuccessResponse(MapToDto(item), "Allocation retrieved successfully");
    }

    public async Task<ApiResponse<AllocationSummaryDto>> GetAllocationSummaryAsync()
    {
        var list = (await _repository.GetAllAsync()).ToList();

        var summary = new AllocationSummaryDto
        {
            ActiveCount = list.Count,
            AwaitingCount = list.Count(a =>
                string.Equals(a.Status, "Pending", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(a.Status, "Partially Allocated", StringComparison.OrdinalIgnoreCase)),
            CompletedCount = list.Count(a =>
                string.Equals(a.Status, "Completed", StringComparison.OrdinalIgnoreCase)),
            ExceptionCount = list.Count(a =>
                string.Equals(a.Status, "Partially Allocated", StringComparison.OrdinalIgnoreCase))
        };

        return ApiResponse<AllocationSummaryDto>.SuccessResponse(summary, "Allocation summary calculated successfully");
    }

    public async Task<ApiResponse<AllocationDto>> CreateAllocationAsync(CreateAllocationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Request) ||
            string.IsNullOrWhiteSpace(request.Resource) ||
            string.IsNullOrWhiteSpace(request.Quantity) ||
            string.IsNullOrWhiteSpace(request.Destination))
        {
            return ApiResponse<AllocationDto>.ErrorResponse("Request ID, Resource, Quantity and Destination are required");
        }

        var newAllocation = new Allocation
        {
            Request = request.Request.Trim(),
            Resource = request.Resource.Trim(),
            Quantity = request.Quantity.Trim(),
            Source = string.IsNullOrWhiteSpace(request.Source) ? "Central Hub" : request.Source.Trim(),
            Destination = request.Destination.Trim(),
            Warehouse = string.IsNullOrWhiteSpace(request.Source) ? "Central Hub" : request.Source.Trim(),
            ApprovedBy = string.IsNullOrWhiteSpace(request.ApprovedBy) ? "Pending Approval" : request.ApprovedBy.Trim(),
            ApprovedAt = DateTime.UtcNow.ToString("dd MMM - HH:mm"),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(newAllocation);
        return ApiResponse<AllocationDto>.SuccessResponse(MapToDto(created), "Allocation created successfully");
    }

    public async Task<ApiResponse<AllocationDto>> UpdateStatusAsync(string id, UpdateAllocationStatusRequest request)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item == null)
        {
            return ApiResponse<AllocationDto>.ErrorResponse($"Allocation with id '{id}' not found");
        }

        item.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.ApprovedBy))
        {
            item.ApprovedBy = request.ApprovedBy;
        }
        item.ApprovedAt = DateTime.UtcNow.ToString("dd MMM - HH:mm");
        item.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(item);
        return ApiResponse<AllocationDto>.SuccessResponse(MapToDto(updated!), "Allocation status updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAllocationAsync(string id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Allocation with id '{id}' not found");
        }
        return ApiResponse<bool>.SuccessResponse(true, "Allocation deleted successfully");
    }

    private static AllocationDto MapToDto(Allocation a) => new()
    {
        Id = a.Id,
        Request = a.Request,
        Resource = a.Resource,
        Quantity = a.Quantity,
        Source = a.Source,
        Destination = a.Destination,
        ApprovedBy = a.ApprovedBy,
        ApprovedAt = a.ApprovedAt,
        Status = a.Status,
        Warehouse = a.Warehouse,
        CreatedAt = a.CreatedAt
    };
}
