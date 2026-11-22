using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IDisasterUpdateService
{
    Task<IEnumerable<DisasterUpdateDto>> GetUpdatesAsync(DisasterUpdateQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<DisasterUpdateDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<DisasterUpdateSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<DisasterUpdateDto> CreateAsync(CreateDisasterUpdateDto dto, CancellationToken cancellationToken = default);
    Task<DisasterUpdateDto?> ToggleArchiveAsync(string id, CancellationToken cancellationToken = default);
    Task<DisasterUpdateDto?> PublishDraftAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmergencyContactDto>> GetEmergencyContactsAsync(CancellationToken cancellationToken = default);
}
