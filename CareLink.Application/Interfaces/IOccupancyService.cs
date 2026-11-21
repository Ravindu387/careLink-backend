using CareLink.Application.DTOs;

namespace CareLink.Application.Interfaces;

public interface IOccupancyService
{
    Task<IEnumerable<OccupancyShelterDto>> GetOccupancySheltersAsync(CancellationToken cancellationToken = default);
    Task<OccupancySummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AdmissionDto>> GetAdmissionsAsync(CancellationToken cancellationToken = default);
    Task<AdmissionDto> RecordAdmissionAsync(RecordAdmissionDto dto, CancellationToken cancellationToken = default);
    Task<OccupancyThresholdDto> GetThresholdsAsync(CancellationToken cancellationToken = default);
    Task<OccupancyThresholdDto> SaveThresholdsAsync(OccupancyThresholdDto dto, CancellationToken cancellationToken = default);
}
