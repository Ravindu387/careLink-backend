using CareLink.Application.DTOs.Auth;
using CareLink.Core.Entities;

namespace CareLink.Application.Interfaces;

public interface IOrganizationDocumentStorageService
{
    (bool IsValid, string? ErrorMessage) ValidateDocument(RegisterDocumentUploadDto document);

    Task<OrganizationDocument> StoreDocumentAsync(
        Guid organizationId,
        RegisterDocumentUploadDto document,
        string uploadedBy,
        CancellationToken cancellationToken = default);

    Task<(byte[] Content, string ContentType, string FileName)?> ReadDocumentAsync(
        OrganizationDocument document,
        CancellationToken cancellationToken = default);

    string SanitizeFileName(string? rawFileName);
}
