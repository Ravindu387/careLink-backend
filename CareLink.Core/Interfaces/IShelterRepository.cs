using CareLink.Core.Entities;

namespace CareLink.Core.Interfaces;

public class ShelterFilterParams
{
    public string? Search { get; set; }
    public string? Area { get; set; }
    public string? Status { get; set; }
    public string? Availability { get; set; } // Available, Full, Any availability
    public string? Service { get; set; }
}

public interface IShelterRepository
{
    Task<IReadOnlyList<Shelter>> GetAllAsync(
        ShelterFilterParams? filter = null,
        CancellationToken cancellationToken = default);

    Task<Shelter?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Shelter> AddAsync(Shelter shelter, CancellationToken cancellationToken = default);

    Task<Shelter?> UpdateAsync(Shelter shelter, CancellationToken cancellationToken = default);

    Task<Shelter?> UpdateOccupancyAsync(
        int id,
        int occupied,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<ShelterMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}
