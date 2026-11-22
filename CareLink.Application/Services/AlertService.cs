namespace CareLink.Application.Services;

using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _repository;

    public AlertService(IAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IEnumerable<AlertDto>>> GetAlertsAsync(string? tab, string? search)
    {
        var alerts = await _repository.GetAllAsync();

        // Filter by tab
        if (!string.IsNullOrWhiteSpace(tab))
        {
            alerts = tab.Trim().ToLowerInvariant() switch
            {
                "open" => alerts.Where(a => string.Equals(a.Status, "Open", StringComparison.OrdinalIgnoreCase)),
                "critical" => alerts.Where(a => string.Equals(a.Severity, "Critical", StringComparison.OrdinalIgnoreCase) && !string.Equals(a.Status, "Resolved", StringComparison.OrdinalIgnoreCase)),
                "assigned to me" or "assignedtome" => alerts.Where(a => string.Equals(a.Status, "Acknowledged", StringComparison.OrdinalIgnoreCase)),
                "acknowledged" => alerts.Where(a => string.Equals(a.Status, "Acknowledged", StringComparison.OrdinalIgnoreCase)),
                "resolved" => alerts.Where(a => string.Equals(a.Status, "Resolved", StringComparison.OrdinalIgnoreCase)),
                _ => alerts
            };
        }

        // Filter by search keyword
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLowerInvariant();
            alerts = alerts.Where(a =>
                (a.Title?.ToLowerInvariant().Contains(keyword) ?? false) ||
                (a.Description?.ToLowerInvariant().Contains(keyword) ?? false) ||
                (a.Location?.ToLowerInvariant().Contains(keyword) ?? false) ||
                (a.Severity?.ToLowerInvariant().Contains(keyword) ?? false)
            );
        }

        var dtos = alerts.Select(MapToDto);
        return ApiResponse<IEnumerable<AlertDto>>.SuccessResponse(dtos, "Alerts retrieved successfully");
    }

    public async Task<ApiResponse<AlertDto>> GetAlertByIdAsync(int id)
    {
        var alert = await _repository.GetByIdAsync(id);
        if (alert == null)
        {
            return ApiResponse<AlertDto>.ErrorResponse($"Alert with id {id} not found");
        }

        return ApiResponse<AlertDto>.SuccessResponse(MapToDto(alert), "Alert retrieved successfully");
    }

    public async Task<ApiResponse<AlertSummaryDto>> GetAlertSummaryAsync()
    {
        var alerts = (await _repository.GetAllAsync()).ToList();

        var summary = new AlertSummaryDto
        {
            TotalCount = alerts.Count,
            CriticalCount = alerts.Count(a =>
                string.Equals(a.Severity, "Critical", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(a.Status, "Resolved", StringComparison.OrdinalIgnoreCase)),
            WarningCount = alerts.Count(a =>
                new[] { "Warning", "Low Stock", "Near Capacity", "Expiring Soon", "Delayed" }
                    .Any(s => string.Equals(s, a.Severity, StringComparison.OrdinalIgnoreCase)) &&
                !string.Equals(a.Status, "Resolved", StringComparison.OrdinalIgnoreCase)),
            AcknowledgedCount = alerts.Count(a =>
                string.Equals(a.Status, "Acknowledged", StringComparison.OrdinalIgnoreCase)),
            ResolvedCount = alerts.Count(a =>
                string.Equals(a.Status, "Resolved", StringComparison.OrdinalIgnoreCase))
        };

        return ApiResponse<AlertSummaryDto>.SuccessResponse(summary, "Alert summary calculated successfully");
    }

    public async Task<ApiResponse<AlertDto>> CreateAlertAsync(CreateAlertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return ApiResponse<AlertDto>.ErrorResponse("Title is required");
        }

        var newAlert = new Alert
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Location = request.Location?.Trim() ?? "General",
            Severity = string.IsNullOrWhiteSpace(request.Severity) ? "Warning" : request.Severity.Trim(),
            Status = "Open",
            Icon = string.IsNullOrWhiteSpace(request.Icon) ? "alert" : request.Icon.Trim(),
            AssignedTo = request.AssignedTo,
            CreatedAt = DateTime.UtcNow,
            Time = "Just now"
        };

        var created = await _repository.CreateAsync(newAlert);
        return ApiResponse<AlertDto>.SuccessResponse(MapToDto(created), "Alert created successfully");
    }

    public async Task<ApiResponse<AlertDto>> AcknowledgeAlertAsync(int id)
    {
        var alert = await _repository.GetByIdAsync(id);
        if (alert == null)
        {
            return ApiResponse<AlertDto>.ErrorResponse($"Alert with id {id} not found");
        }

        alert.Status = "Acknowledged";
        alert.AcknowledgedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(alert);
        return ApiResponse<AlertDto>.SuccessResponse(MapToDto(updated!), "Alert acknowledged successfully");
    }

    public async Task<ApiResponse<AlertDto>> ResolveAlertAsync(int id)
    {
        var alert = await _repository.GetByIdAsync(id);
        if (alert == null)
        {
            return ApiResponse<AlertDto>.ErrorResponse($"Alert with id {id} not found");
        }

        alert.Status = "Resolved";
        alert.ResolvedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(alert);
        return ApiResponse<AlertDto>.SuccessResponse(MapToDto(updated!), "Alert resolved successfully");
    }

    public async Task<ApiResponse<int>> MarkAllReviewedAsync()
    {
        var count = await _repository.MarkAllOpenAsAcknowledgedAsync();
        return ApiResponse<int>.SuccessResponse(count, $"{count} open alerts marked as reviewed");
    }

    public async Task<ApiResponse<AlertRulesDto>> GetRulesAsync()
    {
        var rules = await _repository.GetRulesAsync();
        var dto = new AlertRulesDto
        {
            Critical = rules.Critical,
            LowStock = rules.LowStock,
            Capacity = rules.Capacity,
            Dispatch = rules.Dispatch,
            Expiry = rules.Expiry
        };
        return ApiResponse<AlertRulesDto>.SuccessResponse(dto, "Alert rules retrieved successfully");
    }

    public async Task<ApiResponse<AlertRulesDto>> UpdateRulesAsync(AlertRulesDto rulesDto)
    {
        var rules = new AlertRuleSettings
        {
            Critical = rulesDto.Critical,
            LowStock = rulesDto.LowStock,
            Capacity = rulesDto.Capacity,
            Dispatch = rulesDto.Dispatch,
            Expiry = rulesDto.Expiry
        };

        var updated = await _repository.UpdateRulesAsync(rules);
        var resultDto = new AlertRulesDto
        {
            Critical = updated.Critical,
            LowStock = updated.LowStock,
            Capacity = updated.Capacity,
            Dispatch = updated.Dispatch,
            Expiry = updated.Expiry
        };
        return ApiResponse<AlertRulesDto>.SuccessResponse(resultDto, "Alert rules updated successfully");
    }

    private static AlertDto MapToDto(Alert a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Description = a.Description,
        Location = a.Location,
        Time = a.Time,
        Severity = a.Severity,
        Status = a.Status,
        Icon = a.Icon,
        AssignedTo = a.AssignedTo,
        CreatedAt = a.CreatedAt
    };
}
