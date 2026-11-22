using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IPartnerDocumentService
{
    Task<IReadOnlyList<PartnerDocumentDto>> GetAllAsync(
        PartnerDocumentQueryParameters? query = null,
        CancellationToken cancellationToken = default);

    Task<PartnerDocumentDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<PartnerDocumentDto> CreateAsync(CreatePartnerDocumentDto dto, CancellationToken cancellationToken = default);

    Task<PartnerDocumentDto?> UpdateAsync(string id, UpdatePartnerDocumentDto dto, CancellationToken cancellationToken = default);

    Task<PartnerDocumentDto?> UpdateStatusAsync(string id, UpdatePartnerDocumentStatusDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<PartnerDocumentMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
