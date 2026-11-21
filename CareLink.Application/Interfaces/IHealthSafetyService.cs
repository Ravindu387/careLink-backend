using CareLink.Application.DTOs;
using CareLink.Core.Models;

namespace CareLink.Application.Interfaces;

public interface IHealthSafetyService
{
    Task<IEnumerable<HealthSafetyIncidentDto>> GetIncidentsAsync(HealthSafetyIncidentQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<HealthSafetyIncidentDto?> GetIncidentByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<HealthSafetySummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<HealthSafetyMetaDto> GetMetaAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SafetyCheckDto>> GetSafetyChecksAsync(CancellationToken cancellationToken = default);
    Task<HealthSafetyIncidentDto> CreateIncidentAsync(CreateHealthSafetyIncidentDto dto, CancellationToken cancellationToken = default);
    Task<HealthSafetyIncidentDto?> UpdateIncidentAsync(string id, UpdateHealthSafetyIncidentDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteIncidentAsync(string id, CancellationToken cancellationToken = default);
}
