namespace CareLink.Application.Services;

using System.Globalization;
using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class StockTransferService : IStockTransferService
{
    private readonly IStockTransferRepository _repository;

    public StockTransferService(IStockTransferRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IEnumerable<StockTransferDto>>> GetTransfersAsync(
        string? tab,
        string? search,
        string? source,
        string? destination,
        string? status)
    {
        var transfers = await _repository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(tab))
        {
            var t = tab.Trim().ToLowerInvariant();
            transfers = t switch
            {
                "needs action" or "needsaction" => transfers.Where(x =>
                    string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Status, "Rejected", StringComparison.OrdinalIgnoreCase)),
                "active" => transfers.Where(x =>
                    string.Equals(x.Status, "Approved", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Status, "In Transit", StringComparison.OrdinalIgnoreCase)),
                "completed" => transfers.Where(x =>
                    string.Equals(x.Status, "Completed", StringComparison.OrdinalIgnoreCase)),
                "exceptions" => transfers.Where(x =>
                    string.Equals(x.Status, "Rejected", StringComparison.OrdinalIgnoreCase)),
                _ => transfers
            };
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            transfers = transfers.Where(x =>
                (x.Id?.ToLowerInvariant().Contains(s) ?? false) ||
                (x.Source?.ToLowerInvariant().Contains(s) ?? false) ||
                (x.Destination?.ToLowerInvariant().Contains(s) ?? false) ||
                (x.Resource?.ToLowerInvariant().Contains(s) ?? false) ||
                (x.RequestedBy?.ToLowerInvariant().Contains(s) ?? false)
            );
        }

        if (!string.IsNullOrWhiteSpace(source) && !source.Equals("All source hubs", StringComparison.OrdinalIgnoreCase))
        {
            transfers = transfers.Where(x => string.Equals(x.Source, source, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(destination) && !destination.Equals("All destinations", StringComparison.OrdinalIgnoreCase))
        {
            transfers = transfers.Where(x => string.Equals(x.Destination, destination, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            transfers = transfers.Where(x => string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        var dtos = transfers.Select(MapToDto);
        return ApiResponse<IEnumerable<StockTransferDto>>.SuccessResponse(dtos, "Transfers retrieved successfully");
    }

    public async Task<ApiResponse<StockTransferDto>> GetTransferByIdAsync(string id)
    {
        var transfer = await _repository.GetByIdAsync(id);
        if (transfer == null)
        {
            return ApiResponse<StockTransferDto>.ErrorResponse($"Stock transfer with id '{id}' not found");
        }

        return ApiResponse<StockTransferDto>.SuccessResponse(MapToDto(transfer), "Transfer retrieved successfully");
    }

    public async Task<ApiResponse<StockTransferSummaryDto>> GetSummaryAsync()
    {
        var all = (await _repository.GetAllAsync()).ToList();

        var summary = new StockTransferSummaryDto
        {
            TotalCount = all.Count,
            ActiveCount = all.Count,
            AwaitingCount = all.Count(x => string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase)),
            CompletedCount = all.Count(x => string.Equals(x.Status, "Completed", StringComparison.OrdinalIgnoreCase)),
            ExceptionCount = all.Count(x => string.Equals(x.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
        };

        return ApiResponse<StockTransferSummaryDto>.SuccessResponse(summary, "Transfer summary retrieved successfully");
    }

    public async Task<ApiResponse<StockTransferDto>> CreateTransferAsync(CreateStockTransferRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Source))
        {
            return ApiResponse<StockTransferDto>.ErrorResponse("Source warehouse is required");
        }

        if (string.IsNullOrWhiteSpace(request.Destination))
        {
            return ApiResponse<StockTransferDto>.ErrorResponse("Destination warehouse is required");
        }

        if (string.IsNullOrWhiteSpace(request.Resource))
        {
            return ApiResponse<StockTransferDto>.ErrorResponse("Resource name is required");
        }

        if (string.IsNullOrWhiteSpace(request.Quantity))
        {
            return ApiResponse<StockTransferDto>.ErrorResponse("Quantity is required");
        }

        string scheduledFormatted = request.Scheduled;
        if (DateTime.TryParse(request.Scheduled, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            scheduledFormatted = parsedDate.ToString("dd MMM · HH:mm", CultureInfo.InvariantCulture);
        }

        var entity = new StockTransfer
        {
            Source = request.Source.Trim(),
            Destination = request.Destination.Trim(),
            Resource = request.Resource.Trim(),
            Quantity = request.Quantity.Trim(),
            RequestedBy = string.IsNullOrWhiteSpace(request.RequestedBy) ? "Daniel Perera" : request.RequestedBy.Trim(),
            Scheduled = scheduledFormatted,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(entity);
        return ApiResponse<StockTransferDto>.SuccessResponse(MapToDto(created), "Stock transfer created successfully");
    }

    public async Task<ApiResponse<StockTransferDto>> UpdateStatusAsync(string id, UpdateStockTransferStatusRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<StockTransferDto>.ErrorResponse($"Stock transfer with id '{id}' not found");
        }

        var validStatuses = new[] { "Approved", "In Transit", "Completed", "Rejected", "Pending" };
        var matchedStatus = validStatuses.FirstOrDefault(s => string.Equals(s, request.Status?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (matchedStatus == null)
        {
            return ApiResponse<StockTransferDto>.ErrorResponse($"Invalid status '{request.Status}'. Valid statuses: {string.Join(", ", validStatuses)}");
        }

        existing.Status = matchedStatus;
        var updated = await _repository.UpdateAsync(existing);
        return ApiResponse<StockTransferDto>.SuccessResponse(MapToDto(updated!), "Status updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteTransferAsync(string id)
    {
        var success = await _repository.DeleteAsync(id);
        if (!success)
        {
            return ApiResponse<bool>.ErrorResponse($"Stock transfer with id '{id}' not found");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Stock transfer deleted successfully");
    }

    private static StockTransferDto MapToDto(StockTransfer entity)
    {
        return new StockTransferDto
        {
            Id = entity.Id,
            Source = entity.Source,
            Destination = entity.Destination,
            Resource = entity.Resource,
            Quantity = entity.Quantity,
            RequestedBy = entity.RequestedBy,
            Scheduled = entity.Scheduled,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
