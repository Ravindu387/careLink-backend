using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IDonationService
{
    Task<IEnumerable<DonationDto>> GetAllDonationsAsync(CancellationToken cancellationToken = default);
    Task<PagedResultDto<DonationDto>> GetPagedDonationsAsync(DonationQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<DonationDto?> GetDonationByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<DonationDto> CreateDonationAsync(CreateDonationDto dto, CancellationToken cancellationToken = default);
    Task<DonationDto?> UpdateDonationAsync(string id, UpdateDonationDto dto, CancellationToken cancellationToken = default);
    Task<DonationDto?> UpdateDonationVerificationAsync(string id, string verification, CancellationToken cancellationToken = default);
    Task<DonationDto?> UpdateDonationAllocationAsync(string id, string allocation, CancellationToken cancellationToken = default);
    Task<int> BulkActionAsync(BulkDonationActionDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteDonationAsync(string id, CancellationToken cancellationToken = default);
    Task<DonationSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
