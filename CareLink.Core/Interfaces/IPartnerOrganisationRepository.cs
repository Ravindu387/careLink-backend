using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public class PartnerOrganisationFilterParams
{
    public string? Search { get; set; }
    public string? Region { get; set; }
    public string? Status { get; set; }
    public string? Verification { get; set; }
    public string? Type { get; set; }
    public string? Role { get; set; }
}

public interface IPartnerOrganisationRepository
{
    Task<IReadOnlyList<PartnerOrganisation>> GetAllAsync(
        PartnerOrganisationFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<PartnerOrganisation?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<PartnerOrganisation> AddAsync(PartnerOrganisation organisation, CancellationToken cancellationToken = default);

    Task<PartnerOrganisation?> UpdateAsync(PartnerOrganisation organisation, CancellationToken cancellationToken = default);

    Task<PartnerOrganisation?> UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default);

    Task<PartnerOrganisation?> UpdateVerificationAsync(string id, string verification, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<PartnerOrganisationsMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
