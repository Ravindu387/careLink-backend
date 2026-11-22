using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IPartnerOrganisationService
{
    Task<IReadOnlyList<PartnerOrganisationDto>> GetAllAsync(
        PartnerOrganisationQueryParameters? query = null,
        CancellationToken cancellationToken = default);

    Task<PartnerOrganisationDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<PartnerOrganisationDto> CreateAsync(CreatePartnerOrganisationDto dto, CancellationToken cancellationToken = default);

    Task<PartnerOrganisationDto?> UpdateAsync(string id, UpdatePartnerOrganisationDto dto, CancellationToken cancellationToken = default);

    Task<PartnerOrganisationDto?> UpdateStatusAsync(string id, UpdatePartnerStatusDto dto, CancellationToken cancellationToken = default);

    Task<PartnerOrganisationDto?> UpdateVerificationAsync(string id, UpdatePartnerVerificationDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<PartnerOrganisationsMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetDefaultDataAsync(CancellationToken cancellationToken = default);
}
