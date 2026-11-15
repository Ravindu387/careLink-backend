using CareLink.Application.Common;
using CareLink.Application.DTOs.Integrations;

namespace CareLink.Application.Interfaces;

public interface IIntegrationHealthService
{
    Task<ApiResponse<List<IntegrationHealthDto>>> GetIntegrationsAsync(string? type = null, string? health = null, string? search = null);
    Task<ApiResponse<IntegrationHealthDto>> GetIntegrationByIdAsync(string id);
    Task<ApiResponse<PlatformHealthOverviewDto>> GetOverviewAsync();
    Task<ApiResponse<List<IntegrationLogDto>>> GetIntegrationLogsAsync(string integrationId);
    Task<ApiResponse<List<IntegrationHealthDto>>> RunHealthChecksAsync();
    Task<ApiResponse<IntegrationHealthDto>> RetryIntegrationAsync(string id, IntegrationActionRequestDto request);
    Task<ApiResponse<bool>> RecordActionAsync(string? integrationId, IntegrationActionRequestDto request);
}
