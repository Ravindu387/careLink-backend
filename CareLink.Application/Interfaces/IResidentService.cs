using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IResidentService
{
    Task<PagedResultDto<ResidentDto>> GetPagedResidentsAsync(ResidentQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<IEnumerable<ResidentDto>> GetAllResidentsAsync(CancellationToken cancellationToken = default);
    Task<ResidentDto?> GetResidentByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ResidentSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<ResidentDto> CreateResidentAsync(CreateResidentDto dto, CancellationToken cancellationToken = default);
    Task<ResidentDto?> UpdateResidentAsync(string id, UpdateResidentDto dto, CancellationToken cancellationToken = default);
    Task<ResidentDto?> UpdateStatusAsync(string id, UpdateResidentStatusDto dto, CancellationToken cancellationToken = default);
    Task<ResidentDto?> CheckoutResidentAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> DeleteResidentAsync(string id, CancellationToken cancellationToken = default);
}
