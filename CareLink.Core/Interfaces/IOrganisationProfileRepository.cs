using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public interface IOrganisationProfileRepository
{
    Task<OrganisationProfile> GetProfileAsync(
        string orgId = "ORG-027",
        CancellationToken cancellationToken = default);

    Task<OrganisationProfile> UpdateProfileAsync(
        OrganisationProfile profile,
        CancellationToken cancellationToken = default);

    Task<OrganisationProfile> UpdateCapabilitiesAsync(
        string orgId,
        OrganisationCapabilities capabilities,
        CancellationToken cancellationToken = default);

    Task<OrganisationProfile> ResetDefaultProfileAsync(
        CancellationToken cancellationToken = default);
}
