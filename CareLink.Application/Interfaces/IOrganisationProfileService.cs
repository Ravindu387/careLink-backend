using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IOrganisationProfileService
{
    Task<OrganisationProfileDto> GetProfileAsync(
        string orgId = "ORG-027",
        CancellationToken cancellationToken = default);

    Task<OrganisationProfileDto> UpdateProfileAsync(
        string orgId,
        UpdateOrganisationProfileDto dto,
        CancellationToken cancellationToken = default);

    Task<OrganisationProfileDto> UpdateCapabilitiesAsync(
        string orgId,
        UpdateCapabilitiesDto dto,
        CancellationToken cancellationToken = default);

    Task<OrganisationProfileDto> ResetDefaultProfileAsync(
        CancellationToken cancellationToken = default);
}
