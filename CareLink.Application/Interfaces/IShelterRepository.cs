namespace CareLink.Application.Interfaces;

using CareLink.Core.Entities;

public interface IShelterRepository
{
    Task<IEnumerable<Shelter>> GetSheltersAsync(
        string? search = null,
        string? area = null,
        double? maxDistance = null,
        string? availability = null,
        bool? accessibleOnly = null,
        string? service = null);

    Task<Shelter?> GetByIdAsync(string id);
    Task<Shelter> CreateAsync(Shelter shelter);
    Task<Shelter?> UpdateAsync(string id, Shelter shelter);
    Task<bool> DeleteAsync(string id);
    Task<ShelterBooking> CreateBookingAsync(ShelterBooking booking);
    Task<IEnumerable<ShelterBooking>> GetBookingsAsync(string? citizenName = null);
}
