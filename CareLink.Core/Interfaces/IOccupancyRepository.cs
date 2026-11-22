using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public interface IOccupancyRepository
{
    Task<IEnumerable<Shelter>> GetSheltersAsync(CancellationToken cancellationToken = default);
    Task<Shelter?> GetShelterByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> UpdateShelterAsync(Shelter shelter, CancellationToken cancellationToken = default);

    Task<IEnumerable<Admission>> GetAdmissionsAsync(CancellationToken cancellationToken = default);
    Task<Admission> AddAdmissionAsync(Admission admission, CancellationToken cancellationToken = default);

    Task<OccupancyThreshold?> GetThresholdsAsync(CancellationToken cancellationToken = default);
    Task<OccupancyThreshold> SaveThresholdsAsync(OccupancyThreshold thresholds, CancellationToken cancellationToken = default);
}
