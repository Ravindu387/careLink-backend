namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class DispatchService : IDispatchService
{
    private readonly IDispatchRepository _repository;

    public DispatchService(IDispatchRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<DispatchDto>>> GetDispatchesAsync(DispatchFilterParams? filters = null)
    {
        var list = await _repository.GetAllAsync(filters);
        var dtos = list.Select(MapToDto).ToList();
        return ApiResponse<List<DispatchDto>>.SuccessResponse(dtos, "Dispatches retrieved successfully.");
    }

    public async Task<ApiResponse<DispatchDto>> GetDispatchByIdAsync(string id)
    {
        var dispatch = await _repository.GetByIdAsync(id);
        if (dispatch == null)
        {
            return ApiResponse<DispatchDto>.ErrorResponse($"Dispatch with ID '{id}' was not found.");
        }
        return ApiResponse<DispatchDto>.SuccessResponse(MapToDto(dispatch));
    }

    public async Task<ApiResponse<DispatchDto>> CreateDispatchAsync(CreateDispatchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Resource))
        {
            return ApiResponse<DispatchDto>.ErrorResponse("Resource is required.");
        }
        if (string.IsNullOrWhiteSpace(request.Quantity))
        {
            return ApiResponse<DispatchDto>.ErrorResponse("Quantity is required.");
        }
        if (string.IsNullOrWhiteSpace(request.Vehicle))
        {
            return ApiResponse<DispatchDto>.ErrorResponse("Vehicle is required.");
        }
        if (string.IsNullOrWhiteSpace(request.Driver))
        {
            return ApiResponse<DispatchDto>.ErrorResponse("Driver is required.");
        }
        if (string.IsNullOrWhiteSpace(request.Eta))
        {
            return ApiResponse<DispatchDto>.ErrorResponse("Estimated arrival time (ETA) is required.");
        }

        var route = !string.IsNullOrWhiteSpace(request.Route)
            ? request.Route.Trim()
            : $"{request.From?.Trim()} → {request.To?.Trim()}";

        var departed = !string.IsNullOrWhiteSpace(request.Departed)
            ? request.Departed.Trim()
            : DateTime.UtcNow.ToString("HH:mm");

        var status = !string.IsNullOrWhiteSpace(request.Status)
            ? request.Status.Trim()
            : "Pending";

        var dispatch = new Dispatch
        {
            Route = route,
            Resource = request.Resource.Trim(),
            Quantity = request.Quantity.Trim(),
            Vehicle = request.Vehicle.Trim(),
            Driver = request.Driver.Trim(),
            Departed = departed,
            Eta = request.Eta.Trim(),
            Status = status,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(dispatch);
        return ApiResponse<DispatchDto>.SuccessResponse(MapToDto(created), "Dispatch created successfully.");
    }

    public async Task<ApiResponse<DispatchDto>> UpdateStatusAsync(string id, UpdateDispatchStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return ApiResponse<DispatchDto>.ErrorResponse("Status is required.");
        }

        var validStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "In Transit", "Delayed", "Delivered", "Pending"
        };

        if (!validStatuses.Contains(request.Status.Trim()))
        {
            return ApiResponse<DispatchDto>.ErrorResponse($"Invalid status '{request.Status}'. Valid values are: In Transit, Delayed, Delivered, Pending.");
        }

        var updated = await _repository.UpdateStatusAsync(id, request.Status.Trim(), request.CurrentLocation, request.Notes);
        if (updated == null)
        {
            return ApiResponse<DispatchDto>.ErrorResponse($"Dispatch with ID '{id}' was not found.");
        }

        return ApiResponse<DispatchDto>.SuccessResponse(MapToDto(updated), $"Dispatch status updated to {updated.Status}.");
    }

    public async Task<ApiResponse<bool>> DeleteDispatchAsync(string id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Dispatch with ID '{id}' was not found.");
        }
        return ApiResponse<bool>.SuccessResponse(true, "Dispatch deleted successfully.");
    }

    public async Task<ApiResponse<DispatchSummaryDto>> GetSummaryAsync()
    {
        var summary = await _repository.GetSummaryAsync();
        return ApiResponse<DispatchSummaryDto>.SuccessResponse(summary, "Dispatch summary retrieved successfully.");
    }

    private static DispatchDto MapToDto(Dispatch entity)
    {
        return new DispatchDto
        {
            Id = entity.Id,
            Route = entity.Route,
            Resource = entity.Resource,
            Quantity = entity.Quantity,
            Vehicle = entity.Vehicle,
            Driver = entity.Driver,
            Departed = entity.Departed,
            Eta = entity.Eta,
            Status = entity.Status,
            CurrentLocation = entity.CurrentLocation,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt
        };
    }
}
