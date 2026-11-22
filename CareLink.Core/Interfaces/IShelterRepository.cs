using CareLink.Core.Entities;
using CareLink.Core.Models;

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
        ShelterFilterParams? filter,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Shelter>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Shelter?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Shelter?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<Shelter> AddAsync(Shelter shelter, CancellationToken cancellationToken = default);

    Task<Shelter?> UpdateAsync(Shelter shelter, CancellationToken cancellationToken = default);

    Task<Shelter?> UpdateOccupancyAsync(
        int id,
        int occupied,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<ShelterMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);

    Task<(IEnumerable<Shelter> Items, int TotalCount)> GetPagedAsync(
        ShelterQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<(int Registered, int Operational, int NearCapacity, int Maintenance)> GetSummaryMetricsAsync(
        CancellationToken cancellationToken = default);

    // Citizen Portal & Extended Shelter Management
    Task<IEnumerable<Shelter>> GetSheltersAsync(
        string? search = null,
        string? area = null,
        double? maxDistance = null,
        string? availability = null,
        bool? accessibleOnly = null,
        string? service = null);

    Task<Shelter> CreateAsync(Shelter shelter);

    Task<Shelter?> UpdateAsync(string id, Shelter shelter);

    Task<ShelterBooking> CreateBookingAsync(ShelterBooking booking);

    Task<IEnumerable<ShelterBooking>> GetBookingsAsync(string? citizenName = null);
}
