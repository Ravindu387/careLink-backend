namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface IAlertService
{
    Task<ApiResponse<IEnumerable<AlertDto>>> GetAlertsAsync(string? tab, string? search);
    Task<ApiResponse<AlertDto>> GetAlertByIdAsync(int id);
    Task<ApiResponse<AlertSummaryDto>> GetAlertSummaryAsync();
    Task<ApiResponse<AlertDto>> CreateAlertAsync(CreateAlertRequest request);
    Task<ApiResponse<AlertDto>> AcknowledgeAlertAsync(int id);
    Task<ApiResponse<AlertDto>> ResolveAlertAsync(int id);
    Task<ApiResponse<int>> MarkAllReviewedAsync();
    Task<ApiResponse<AlertRulesDto>> GetRulesAsync();
    Task<ApiResponse<AlertRulesDto>> UpdateRulesAsync(AlertRulesDto rulesDto);
}
