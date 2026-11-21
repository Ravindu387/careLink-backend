using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IMaintenanceRequestService
{
    Task<PagedResultDto<MaintenanceRequestDto>> GetPagedRequestsAsync(MaintenanceRequestQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceRequestDto>> GetAllRequestsAsync(CancellationToken cancellationToken = default);
    Task<MaintenanceRequestDto?> GetRequestByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<MaintenanceRequestSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<MaintenanceRequestMetaDto> GetMetaAsync(CancellationToken cancellationToken = default);
    Task<MaintenanceRequestDto> CreateRequestAsync(CreateMaintenanceRequestDto dto, CancellationToken cancellationToken = default);
    Task<MaintenanceRequestDto?> UpdateRequestAsync(string id, UpdateMaintenanceRequestDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteRequestAsync(string id, CancellationToken cancellationToken = default);
}
