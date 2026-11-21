namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class ResourceRequestService : IResourceRequestService
{
    private readonly IResourceRequestRepository _repository;
    private readonly IAuditLogRepository _auditLogRepository;

    public ResourceRequestService(
        IResourceRequestRepository repository,
        IAuditLogRepository auditLogRepository)
    {
        _repository = repository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<ApiResponse<List<ResourceRequestDto>>> GetRequestsAsync(ResourceRequestFilterParams? filters = null)
    {
        var items = await _repository.GetAllAsync(filters);
        var dtos = items.Select(MapToDto).ToList();
        return ApiResponse<List<ResourceRequestDto>>.SuccessResponse(dtos, "Resource requests retrieved.");
    }

    public async Task<ApiResponse<ResourceRequestDto>> GetRequestByIdAsync(string id)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item == null)
        {
            return ApiResponse<ResourceRequestDto>.ErrorResponse($"Resource request '{id}' was not found.");
        }
        return ApiResponse<ResourceRequestDto>.SuccessResponse(MapToDto(item));
    }

    public async Task<ApiResponse<ResourceRequestSummaryDto>> GetSummaryAsync()
    {
        var summary = await _repository.GetSummaryAsync();
        return ApiResponse<ResourceRequestSummaryDto>.SuccessResponse(summary, "Resource requests summary retrieved.");
    }

    public async Task<ApiResponse<ResourceRequestDto>> CreateRequestAsync(CreateResourceRequestPayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload.Unit))
        {
            return ApiResponse<ResourceRequestDto>.ErrorResponse("Requesting unit is required.");
        }
        if (string.IsNullOrWhiteSpace(payload.Resource))
        {
            return ApiResponse<ResourceRequestDto>.ErrorResponse("Resource name is required.");
        }

        var entity = new ResourceRequest
        {
            Unit = payload.Unit.Trim(),
            Resource = payload.Resource.Trim(),
            Quantity = string.IsNullOrWhiteSpace(payload.Quantity) ? "100 units" : payload.Quantity.Trim(),
            Priority = string.IsNullOrWhiteSpace(payload.Priority) ? "Medium" : payload.Priority.Trim(),
            Category = string.IsNullOrWhiteSpace(payload.Category) ? "Food & Water" : payload.Category.Trim(),
            Source = string.IsNullOrWhiteSpace(payload.Source) ? "Pending Assignment" : payload.Source.Trim(),
            Age = "Just now",
            Status = "Awaiting Approval",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity);

        try
        {
            await _auditLogRepository.AddAsync(new AuditLog
            {
                Action = "Resource request created",
                Actor = "Daniel Perera",
                Record = created.Id,
                Change = $"{created.Quantity} {created.Resource} for {created.Unit}",
                Rationale = "Operational emergency requisition",
                ActionType = "Verification",
                RecordType = "Request",
                Timestamp = $"Today · {DateTime.UtcNow:HH:mm}",
                CreatedAt = DateTime.UtcNow
            });
        }
        catch
        {
            // Non-critical audit logging failure fallback
        }

        return ApiResponse<ResourceRequestDto>.SuccessResponse(MapToDto(created), "Resource request created successfully.");
    }

    public async Task<ApiResponse<ResourceRequestDto>> UpdateRequestStatusAsync(string id, UpdateResourceRequestStatusPayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload.Status))
        {
            return ApiResponse<ResourceRequestDto>.ErrorResponse("Status is required.");
        }

        var updated = await _repository.UpdateStatusAsync(id, payload.Status, payload.Source);
        if (updated == null)
        {
            return ApiResponse<ResourceRequestDto>.ErrorResponse($"Resource request '{id}' was not found.");
        }

        try
        {
            await _auditLogRepository.AddAsync(new AuditLog
            {
                Action = $"Request {id} status updated",
                Actor = "Daniel Perera",
                Record = id,
                Change = $"Status changed to {payload.Status}" + (!string.IsNullOrWhiteSpace(payload.Source) ? $", Source: {payload.Source}" : ""),
                Rationale = "Management operational transition",
                ActionType = "Approval",
                RecordType = "Request",
                Timestamp = $"Today · {DateTime.UtcNow:HH:mm}",
                CreatedAt = DateTime.UtcNow
            });
        }
        catch
        {
            // Non-critical audit logging failure fallback
        }

        return ApiResponse<ResourceRequestDto>.SuccessResponse(MapToDto(updated), $"Request '{id}' updated to '{payload.Status}'.");
    }

    public async Task<ApiResponse<bool>> DeleteRequestAsync(string id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Resource request '{id}' was not found.");
        }
        return ApiResponse<bool>.SuccessResponse(true, $"Request '{id}' removed successfully.");
    }

    public async Task<(byte[] Bytes, string ContentType, string FileName)> ExportRequestsAsync(ResourceRequestFilterParams? filters = null, string format = "csv")
    {
        var requests = (await _repository.GetAllAsync(filters)).ToList();
        var headers = new[] { "Request ID", "Requesting Unit", "Resource", "Quantity", "Priority", "Category", "Suggested Source", "Age", "Status" };
        var rows = requests.Select(r => new[]
        {
            r.Id,
            r.Unit,
            r.Resource,
            r.Quantity,
            r.Priority,
            r.Category,
            r.Source,
            r.Age,
            r.Status
        }).ToList();

        var isPdf = string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase);
        var title = "CareLink Resource Requests Ledger";
        var subtitle = $"Total Requests: {requests.Count} | Exported on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";

        if (isPdf)
        {
            var bytes = ReportDocumentGenerator.GeneratePdf(title, subtitle, rows, headers);
            return (bytes, "application/pdf", $"carelink-resource-requests-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        }
        else
        {
            var bytes = ReportDocumentGenerator.GenerateCsv(rows, headers);
            return (bytes, "text/csv; charset=utf-8", $"carelink-resource-requests-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }
    }

    private static ResourceRequestDto MapToDto(ResourceRequest entity)
    {
        return new ResourceRequestDto
        {
            Id = entity.Id,
            Unit = entity.Unit,
            Resource = entity.Resource,
            Quantity = entity.Quantity,
            Priority = entity.Priority,
            Category = entity.Category,
            Source = entity.Source,
            Age = entity.Age,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt
        };
    }
}
