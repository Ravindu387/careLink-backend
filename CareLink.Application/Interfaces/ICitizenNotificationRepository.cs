namespace CareLink.Application.Interfaces;

using CareLink.Core.Entities;

public interface ICitizenNotificationRepository
{
    Task<IEnumerable<CitizenNotification>> GetAllAsync(
        string? citizenName = null,
        string? category = null,
        bool? unreadOnly = null,
        string? search = null);

    Task<CitizenNotification?> GetByIdAsync(string id);
    Task<CitizenNotification> CreateAsync(CitizenNotification notification);
    Task<bool> MarkAsReadAsync(string id);
    Task<int> MarkAllAsReadAsync(string? citizenName = null);
    Task<bool> DeleteAsync(string id);
}
