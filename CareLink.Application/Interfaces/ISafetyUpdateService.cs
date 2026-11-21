namespace CareLink.Application.Interfaces;

using CareLink.Application.Common;
using CareLink.Application.DTOs;

public interface ISafetyUpdateService
{
    Task<ApiResponse<IEnumerable<SafetyUpdateDto>>> GetUpdatesAsync(string? area = null, string? level = null, string? search = null);
    Task<ApiResponse<SafetyUpdateDto>> GetByIdAsync(string id);
    Task<ApiResponse<SafetyUpdateDto>> CreateAsync(CreateSafetyUpdateRequest request);
    Task<ApiResponse<SafetyUpdateDto>> UpdateAsync(string id, UpdateSafetyUpdateRequest request);
    Task<ApiResponse<bool>> DeleteAsync(string id);
    Task<ApiResponse<SafetyUpdateSummaryDto>> GetSummaryAsync(string? area = null);
    Task<ApiResponse<IEnumerable<string>>> GetMonitoredAreasAsync();
}
