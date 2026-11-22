namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class CitizenRequestService : ICitizenRequestService
{
    private readonly ICitizenRequestRepository _repository;

    public CitizenRequestService(ICitizenRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IEnumerable<CitizenHelpRequestDto>>> GetRequestsAsync(string? citizenName = null, string? search = null, string? status = null, string? category = null)
    {
        var entities = await _repository.GetRequestsAsync(citizenName, search, status, category);
        var dtos = entities.Select(MapToDto).ToList();
        return ApiResponse<IEnumerable<CitizenHelpRequestDto>>.SuccessResponse(dtos, "Requests retrieved successfully.");
    }

    public async Task<ApiResponse<CitizenHelpRequestDto>> GetByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            return ApiResponse<CitizenHelpRequestDto>.ErrorResponse($"Request with ID '{id}' was not found.");
        }
        return ApiResponse<CitizenHelpRequestDto>.SuccessResponse(MapToDto(entity), "Request retrieved successfully.");
    }

    public async Task<ApiResponse<CitizenHelpRequestDto>> CreateRequestAsync(CreateCitizenHelpRequestPayload request, string? citizenName = null)
    {
        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return ApiResponse<CitizenHelpRequestDto>.ErrorResponse("Help category is required.");
        }

        var description = !string.IsNullOrWhiteSpace(request.Situation)
            ? request.Situation.Trim()
            : (!string.IsNullOrWhiteSpace(request.Description) ? request.Description.Trim() : $"{request.Category} requested.");

        var applicantName = !string.IsNullOrWhiteSpace(request.ContactName)
            ? request.ContactName.Trim()
            : (!string.IsNullOrWhiteSpace(request.CitizenName)
                ? request.CitizenName.Trim()
                : (!string.IsNullOrWhiteSpace(citizenName) ? citizenName.Trim() : "Samira Khan"));

        var applicantPhone = !string.IsNullOrWhiteSpace(request.Phone)
            ? request.Phone.Trim()
            : (!string.IsNullOrWhiteSpace(request.ContactPhone) ? request.ContactPhone.Trim() : "+94 77 123 4567");

        var applicantAddress = !string.IsNullOrWhiteSpace(request.Address)
            ? request.Address.Trim()
            : "North Ward, Greater Riverton";

        var priority = request.ImmediateThreat
            ? "Urgent"
            : (!string.IsNullOrWhiteSpace(request.Priority) ? request.Priority.Trim() : "Normal");

        var color = request.ImmediateThreat ? "red" : GetColorForCategory(request.Category);

        var entity = new CitizenRequest
        {
            Title = !string.IsNullOrWhiteSpace(request.Title) ? request.Title.Trim() : request.Category.Trim(),
            Category = request.Category.Trim(),
            Description = description,
            Status = "Under review",
            Priority = priority,
            CitizenName = applicantName,
            ContactPhone = applicantPhone,
            Address = applicantAddress,
            District = request.District?.Trim(),
            Color = color,
            ImmediateThreat = request.ImmediateThreat,
            AdditionalInfo = request.AdditionalInfo?.Trim(),
            PeopleCount = request.PeopleCount ?? 1,
            Progress = 2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateRequestAsync(entity);
        return ApiResponse<CitizenHelpRequestDto>.SuccessResponse(MapToDto(created), "Help request submitted successfully.");
    }

    public async Task<ApiResponse<CitizenHelpRequestDto>> UpdateStatusAsync(string id, string status, int? progress = null)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return ApiResponse<CitizenHelpRequestDto>.ErrorResponse("Status cannot be empty.");
        }

        var updated = await _repository.UpdateStatusAsync(id, status, progress);
        if (updated == null)
        {
            return ApiResponse<CitizenHelpRequestDto>.ErrorResponse($"Request with ID '{id}' was not found.");
        }

        return ApiResponse<CitizenHelpRequestDto>.SuccessResponse(MapToDto(updated), "Request status updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteRequestAsync(string id)
    {
        var deleted = await _repository.DeleteRequestAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Request with ID '{id}' could not be deleted.");
        }
        return ApiResponse<bool>.SuccessResponse(true, "Request deleted successfully.");
    }

    public async Task<ApiResponse<CitizenRequestsSummaryDto>> GetSummaryAsync(string? citizenName = null)
    {
        var summary = await _repository.GetSummaryAsync(citizenName);
        return ApiResponse<CitizenRequestsSummaryDto>.SuccessResponse(summary, "Requests summary retrieved successfully.");
    }

    private static string GetColorForCategory(string category)
    {
        return category.ToLowerInvariant() switch
        {
            var c when c.Contains("shelter") => "amber",
            var c when c.Contains("food") || c.Contains("water") => "blue",
            var c when c.Contains("medical") => "green",
            var c when c.Contains("transport") => "indigo",
            var c when c.Contains("family") => "purple",
            _ => "blue"
        };
    }

    private static string FormatTimeAgo(DateTime dt)
    {
        var diff = DateTime.UtcNow - dt;
        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} minutes ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hours ago";
        if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
        return dt.ToString("dd MMM yyyy");
    }

    private static CitizenHelpRequestDto MapToDto(CitizenRequest entity)
    {
        return new CitizenHelpRequestDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Category = entity.Category,
            Description = entity.Description,
            Status = entity.Status,
            Priority = entity.Priority,
            CitizenName = entity.CitizenName,
            ContactPhone = entity.ContactPhone,
            Address = entity.Address,
            District = entity.District,
            Color = entity.Color,
            ImmediateThreat = entity.ImmediateThreat,
            AdditionalInfo = entity.AdditionalInfo,
            PeopleCount = entity.PeopleCount,
            Progress = entity.Progress,
            TimeAgo = FormatTimeAgo(entity.CreatedAt),
            FormattedDate = $"Submitted {entity.CreatedAt:dd MMM yyyy · HH:mm}",
            Updated = entity.UpdatedAt.HasValue ? FormatTimeAgo(entity.UpdatedAt.Value) : "Recently",
            CanMessage = !entity.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) &&
                         !entity.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase),
            DispatchTrackingId = entity.DispatchTrackingId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
