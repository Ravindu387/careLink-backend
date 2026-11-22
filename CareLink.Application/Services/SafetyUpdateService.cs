namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class SafetyUpdateService : ISafetyUpdateService
{
    private readonly ISafetyUpdateRepository _repository;

    public SafetyUpdateService(ISafetyUpdateRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IEnumerable<SafetyUpdateDto>>> GetUpdatesAsync(string? area = null, string? level = null, string? search = null)
    {
        var entities = await _repository.GetUpdatesAsync(area, level, search);
        var dtos = entities.Select(MapToDto).ToList();
        return ApiResponse<IEnumerable<SafetyUpdateDto>>.SuccessResponse(dtos, "Safety updates retrieved successfully.");
    }

    public async Task<ApiResponse<SafetyUpdateDto>> GetByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            return ApiResponse<SafetyUpdateDto>.ErrorResponse($"Safety update with ID '{id}' was not found.");
        }
        return ApiResponse<SafetyUpdateDto>.SuccessResponse(MapToDto(entity), "Safety update retrieved successfully.");
    }

    public async Task<ApiResponse<SafetyUpdateDto>> CreateAsync(CreateSafetyUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return ApiResponse<SafetyUpdateDto>.ErrorResponse("Title is required for safety updates.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return ApiResponse<SafetyUpdateDto>.ErrorResponse("Description is required for safety updates.");
        }

        var level = !string.IsNullOrWhiteSpace(request.Level) ? request.Level.Trim() : "Emergency alert";
        var severity = !string.IsNullOrWhiteSpace(request.Severity)
            ? request.Severity.Trim()
            : (level.Equals("Emergency alert", StringComparison.OrdinalIgnoreCase) ? "Critical" : "High");

        var entity = new SafetyUpdate
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Level = level,
            Type = !string.IsNullOrWhiteSpace(request.Type) ? request.Type.Trim() : level,
            Severity = severity,
            Location = !string.IsNullOrWhiteSpace(request.Location) ? request.Location.Trim() : "Greater Riverton",
            Area = !string.IsNullOrWhiteSpace(request.Area) ? request.Area.Trim() : "Greater Riverton",
            Source = !string.IsNullOrWhiteSpace(request.Source) ? request.Source.Trim() : "Disaster Management Centre (DMC)",
            ActionInstructions = request.ActionInstructions?.Trim(),
            SafeRoutes = request.SafeRoutes ?? new List<string>(),
            HazardAreas = request.HazardAreas ?? new List<string>(),
            Helpline = !string.IsNullOrWhiteSpace(request.Helpline) ? request.Helpline.Trim() : "112",
            IsActive = true,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(entity);
        return ApiResponse<SafetyUpdateDto>.SuccessResponse(MapToDto(created), "Safety update created successfully.");
    }

    public async Task<ApiResponse<SafetyUpdateDto>> UpdateAsync(string id, UpdateSafetyUpdateRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<SafetyUpdateDto>.ErrorResponse($"Safety update with ID '{id}' was not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Title)) existing.Title = request.Title.Trim();
        if (!string.IsNullOrWhiteSpace(request.Description)) existing.Description = request.Description.Trim();
        if (!string.IsNullOrWhiteSpace(request.Level)) existing.Level = request.Level.Trim();
        if (!string.IsNullOrWhiteSpace(request.Severity)) existing.Severity = request.Severity.Trim();
        if (!string.IsNullOrWhiteSpace(request.Location)) existing.Location = request.Location.Trim();
        if (!string.IsNullOrWhiteSpace(request.Area)) existing.Area = request.Area.Trim();
        if (!string.IsNullOrWhiteSpace(request.Source)) existing.Source = request.Source.Trim();
        if (request.ActionInstructions != null) existing.ActionInstructions = request.ActionInstructions.Trim();
        if (request.SafeRoutes != null) existing.SafeRoutes = request.SafeRoutes;
        if (request.HazardAreas != null) existing.HazardAreas = request.HazardAreas;
        if (request.IsActive.HasValue) existing.IsActive = request.IsActive.Value;
        if (request.ExpiresAt.HasValue) existing.ExpiresAt = request.ExpiresAt.Value;

        var updated = await _repository.UpdateAsync(id, existing);
        return ApiResponse<SafetyUpdateDto>.SuccessResponse(MapToDto(updated!), "Safety update updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            return ApiResponse<bool>.ErrorResponse($"Safety update with ID '{id}' was not found or could not be deleted.");
        }
        return ApiResponse<bool>.SuccessResponse(true, "Safety update deleted successfully.");
    }

    public async Task<ApiResponse<SafetyUpdateSummaryDto>> GetSummaryAsync(string? area = null)
    {
        var allUpdates = (await _repository.GetUpdatesAsync(area, null, null)).ToList();
        var areas = (await _repository.GetMonitoredAreasAsync()).ToList();

        var summary = new SafetyUpdateSummaryDto
        {
            TotalUpdates = allUpdates.Count,
            EmergencyCount = allUpdates.Count(u => u.Level.Equals("Emergency alert", StringComparison.OrdinalIgnoreCase)),
            WarningsCount = allUpdates.Count(u => u.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase)),
            ServiceUpdatesCount = allUpdates.Count(u => u.Level.Equals("Service update", StringComparison.OrdinalIgnoreCase)),
            MonitoredAreas = areas,
            LastUpdatedAt = DateTime.UtcNow
        };

        return ApiResponse<SafetyUpdateSummaryDto>.SuccessResponse(summary, "Safety update summary retrieved.");
    }

    public async Task<ApiResponse<IEnumerable<string>>> GetMonitoredAreasAsync()
    {
        var areas = await _repository.GetMonitoredAreasAsync();
        return ApiResponse<IEnumerable<string>>.SuccessResponse(areas, "Monitored areas retrieved.");
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

    private static SafetyUpdateDto MapToDto(SafetyUpdate entity)
    {
        return new SafetyUpdateDto
        {
            Id = entity.Id,
            Type = entity.Type,
            Level = entity.Level,
            Severity = entity.Severity,
            Title = entity.Title,
            Description = entity.Description,
            Location = entity.Location,
            Area = entity.Area,
            Time = FormatTimeAgo(entity.CreatedAt),
            Source = entity.Source,
            ActionInstructions = entity.ActionInstructions,
            SafeRoutes = entity.SafeRoutes,
            HazardAreas = entity.HazardAreas,
            Helpline = entity.Helpline,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ExpiresAt = entity.ExpiresAt
        };
    }
}
