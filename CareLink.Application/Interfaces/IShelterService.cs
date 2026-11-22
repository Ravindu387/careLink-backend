using CareLink.Application.Common;
using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IShelterService
{
    // Operational & Coordinator Methods (Safna)
    Task<IReadOnlyList<ShelterDto>> GetAllAsync(
        ShelterQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<ShelterDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ShelterDto> CreateAsync(CreateShelterDto dto, CancellationToken cancellationToken = default);

    Task<ShelterDto?> UpdateAsync(int id, UpdateShelterDto dto, CancellationToken cancellationToken = default);

    Task<ShelterDto?> UpdateOccupancyAsync(int id, int occupied, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<ShelterMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);

    // Shelter Management Methods (Prabhani)
    Task<IEnumerable<ShelterDto>> GetAllSheltersAsync(CancellationToken cancellationToken = default);
    Task<ShelterDto?> GetShelterByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<PagedResultDto<ShelterDto>> GetSheltersPagedAsync(ShelterQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<ShelterSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<ShelterDto> CreateShelterAsync(CreateShelterDto dto, CancellationToken cancellationToken = default);

    // Citizen & Community Methods (Oshadi)
    Task<ApiResponse<IEnumerable<ShelterDto>>> GetSheltersAsync(
        string? search = null,
        string? area = null,
        double? maxDistance = null,
        string? availability = null,
        bool? accessibleOnly = null,
        string? service = null);

    Task<ApiResponse<ShelterDto>> GetByIdAsync(string id);
    Task<ApiResponse<ShelterDto>> CreateAsync(CreateShelterRequest request);
    Task<ApiResponse<ShelterDto>> UpdateAsync(string id, UpdateShelterRequest request);
    Task<ApiResponse<bool>> DeleteAsync(string id);
    Task<ApiResponse<ShelterSummaryDto>> GetCitizenSummaryAsync(string? citizenLocation = null);
    Task<ApiResponse<ShelterBookingDto>> RequestSpaceAsync(RequestShelterSpacePayload request, string? citizenName = null);
    Task<ApiResponse<IEnumerable<ShelterBookingDto>>> GetCitizenBookingsAsync(string? citizenName = null);
}
