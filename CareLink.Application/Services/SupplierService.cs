namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repository;

    public SupplierService(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IEnumerable<SupplierDto>>> GetSuppliersAsync(
        string? tab,
        string? search,
        string? category,
        string? region,
        string? verification,
        string? status)
    {
        var suppliers = await _repository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(tab))
        {
            var t = tab.Trim().ToLowerInvariant();
            suppliers = t switch
            {
                "needs action" or "needsaction" => suppliers.Where(x =>
                    string.Equals(x.Verification, "Pending", StringComparison.OrdinalIgnoreCase)),
                "active" => suppliers.Where(x =>
                    string.Equals(x.Status, "Active", StringComparison.OrdinalIgnoreCase)),
                "completed" => suppliers.Where(x =>
                    string.Equals(x.Verification, "Verified", StringComparison.OrdinalIgnoreCase)),
                "exceptions" => suppliers.Where(x =>
                    string.Equals(x.Verification, "Rejected", StringComparison.OrdinalIgnoreCase)),
                _ => suppliers
            };
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            suppliers = suppliers.Where(x =>
                (x.Id?.ToLowerInvariant().Contains(s) ?? false) ||
                (x.Name?.ToLowerInvariant().Contains(s) ?? false) ||
                (x.Category?.ToLowerInvariant().Contains(s) ?? false) ||
                (x.Coverage?.ToLowerInvariant().Contains(s) ?? false) ||
                (x.Verification?.ToLowerInvariant().Contains(s) ?? false) ||
                (x.Status?.ToLowerInvariant().Contains(s) ?? false)
            );
        }

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All categories", StringComparison.OrdinalIgnoreCase))
        {
            suppliers = suppliers.Where(x => string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(region) && !region.Equals("All regions", StringComparison.OrdinalIgnoreCase))
        {
            suppliers = suppliers.Where(x => string.Equals(x.Coverage, region, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(verification) && !verification.Equals("All verification states", StringComparison.OrdinalIgnoreCase))
        {
            suppliers = suppliers.Where(x => string.Equals(x.Verification, verification, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All statuses", StringComparison.OrdinalIgnoreCase))
        {
            suppliers = suppliers.Where(x => string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        var dtos = suppliers.Select(MapToDto);
        return ApiResponse<IEnumerable<SupplierDto>>.SuccessResponse(dtos, "Suppliers retrieved successfully");
    }

    public async Task<ApiResponse<SupplierDto>> GetSupplierByIdAsync(string id)
    {
        var supplier = await _repository.GetByIdAsync(id);
        if (supplier == null)
        {
            return ApiResponse<SupplierDto>.ErrorResponse($"Supplier with id '{id}' not found");
        }

        return ApiResponse<SupplierDto>.SuccessResponse(MapToDto(supplier), "Supplier retrieved successfully");
    }

    public async Task<ApiResponse<SupplierSummaryDto>> GetSummaryAsync()
    {
        var all = (await _repository.GetAllAsync()).ToList();

        var summary = new SupplierSummaryDto
        {
            TotalCount = all.Count,
            ActiveCount = all.Count(x => string.Equals(x.Status, "Active", StringComparison.OrdinalIgnoreCase)),
            PendingCount = all.Count(x => string.Equals(x.Verification, "Pending", StringComparison.OrdinalIgnoreCase)),
            VerifiedCount = all.Count(x => string.Equals(x.Verification, "Verified", StringComparison.OrdinalIgnoreCase)),
            ExceptionCount = all.Count(x => string.Equals(x.Verification, "Rejected", StringComparison.OrdinalIgnoreCase))
        };

        return ApiResponse<SupplierSummaryDto>.SuccessResponse(summary, "Supplier summary retrieved successfully");
    }

    public async Task<ApiResponse<SupplierDto>> CreateSupplierAsync(CreateSupplierRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiResponse<SupplierDto>.ErrorResponse("Supplier name is required");
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return ApiResponse<SupplierDto>.ErrorResponse("Supplier category is required");
        }

        if (string.IsNullOrWhiteSpace(request.Coverage))
        {
            return ApiResponse<SupplierDto>.ErrorResponse("Supplier coverage/region is required");
        }

        var onTimeFormatted = request.OnTimeRate?.Trim() ?? "95%";
        if (!onTimeFormatted.EndsWith("%"))
        {
            onTimeFormatted += "%";
        }

        var entity = new Supplier
        {
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            Coverage = request.Coverage.Trim(),
            OnTimeRate = onTimeFormatted,
            LeadTime = string.IsNullOrWhiteSpace(request.LeadTime) ? "2.0 days" : request.LeadTime.Trim(),
            Verification = string.IsNullOrWhiteSpace(request.Verification) ? "Pending" : request.Verification.Trim(),
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(entity);
        return ApiResponse<SupplierDto>.SuccessResponse(MapToDto(created), "Supplier created successfully");
    }

    public async Task<ApiResponse<SupplierDto>> UpdateStatusAsync(string id, UpdateSupplierStatusRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<SupplierDto>.ErrorResponse($"Supplier with id '{id}' not found");
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var validStatuses = new[] { "Active", "Inactive", "Suspended" };
            var matched = validStatuses.FirstOrDefault(s => string.Equals(s, request.Status.Trim(), StringComparison.OrdinalIgnoreCase));
            if (matched != null)
            {
                existing.Status = matched;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Verification))
        {
            var validVerifications = new[] { "Verified", "Pending", "Rejected" };
            var matched = validVerifications.FirstOrDefault(v => string.Equals(v, request.Verification.Trim(), StringComparison.OrdinalIgnoreCase));
            if (matched != null)
            {
                existing.Verification = matched;
            }
        }

        var updated = await _repository.UpdateAsync(existing);
        return ApiResponse<SupplierDto>.SuccessResponse(MapToDto(updated!), "Supplier updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteSupplierAsync(string id)
    {
        var success = await _repository.DeleteAsync(id);
        if (!success)
        {
            return ApiResponse<bool>.ErrorResponse($"Supplier with id '{id}' not found");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Supplier deleted successfully");
    }

    private static SupplierDto MapToDto(Supplier entity)
    {
        return new SupplierDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Category = entity.Category,
            Coverage = entity.Coverage,
            OnTimeRate = entity.OnTimeRate,
            LeadTime = entity.LeadTime,
            Verification = entity.Verification,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
