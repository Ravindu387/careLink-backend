using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IDonorService
{
    Task<IEnumerable<DonorDto>> GetAllDonorsAsync(CancellationToken cancellationToken = default);
    Task<PagedResultDto<DonorDto>> GetPagedDonorsAsync(DonorQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<DonorDto?> GetDonorByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<DonorDto> CreateDonorAsync(CreateDonorDto dto, CancellationToken cancellationToken = default);
    Task<DonorDto?> UpdateDonorAsync(string id, UpdateDonorDto dto, CancellationToken cancellationToken = default);
    Task<DonorDto?> UpdateDonorStatusAsync(string id, string status, CancellationToken cancellationToken = default);
    Task<DonorDto?> UpdateDonorVerificationAsync(string id, string verification, CancellationToken cancellationToken = default);
    Task<int> BulkActionAsync(BulkDonorActionDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteDonorAsync(string id, CancellationToken cancellationToken = default);
    Task<DonorSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
