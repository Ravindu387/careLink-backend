using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly CareLinkDbContext _context;

    public NotificationRepository(CareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<NotificationItem> Items, int TotalCount)> GetPagedAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(parameters);

        var totalCount = await query.CountAsync(cancellationToken);

        var page = parameters.Page > 0 ? parameters.Page : 1;
        var pageSize = parameters.PageSize > 0 ? parameters.PageSize : 6;

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<NotificationItem>> GetAllAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(parameters);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<NotificationItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<NotificationItem> AddAsync(NotificationItem item, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(item, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<bool> UpdateAsync(NotificationItem item, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == item.Id, cancellationToken);
        if (existing == null) return false;

        if (!ReferenceEquals(existing, item))
        {
            _context.Entry(existing).CurrentValues.SetValues(item);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        if (existing == null) return false;

        existing.Unread = false;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications.Where(n => n.Unread);
        if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.Role == role || n.Role == "All");
        }

        var unreadItems = await query.ToListAsync(cancellationToken);
        if (unreadItems.Count == 0) return 0;

        foreach (var item in unreadItems)
        {
            item.Unread = false;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return unreadItems.Count;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        if (existing == null) return false;

        _context.Notifications.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> GetUnreadCountAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications.AsNoTracking().Where(n => n.Unread);
        if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.Role == role || n.Role == "All");
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetTypeCountsAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.Role == role || n.Role == "All");
        }

        return await query
            .GroupBy(n => n.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);
    }

    public async Task<NotificationPreference?> GetPreferencesAsync(string role, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.Role == role, cancellationToken);
    }

    public async Task<NotificationPreference> SavePreferencesAsync(NotificationPreference preference, CancellationToken cancellationToken = default)
    {
        var existing = await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.Role == preference.Role, cancellationToken);

        if (existing == null)
        {
            await _context.NotificationPreferences.AddAsync(preference, cancellationToken);
        }
        else
        {
            if (!ReferenceEquals(existing, preference))
            {
                _context.Entry(existing).CurrentValues.SetValues(preference);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return preference;
    }

    private IQueryable<NotificationItem> BuildQuery(NotificationQueryParameters parameters)
    {
        var query = _context.Notifications.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Role) &&
            !parameters.Role.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.Role == parameters.Role || n.Role == "All");
        }

        if (!string.IsNullOrWhiteSpace(parameters.Type) &&
            !parameters.Type.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var type = parameters.Type.Trim();
            query = query.Where(n => n.Type.ToLower() == type.ToLower());
        }

        if (parameters.Unread.HasValue)
        {
            query = query.Where(n => n.Unread == parameters.Unread.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var term = parameters.Search.Trim().ToLower();
            query = query.Where(n =>
                n.Title.ToLower().Contains(term) ||
                n.Description.ToLower().Contains(term) ||
                n.Reference.ToLower().Contains(term) ||
                n.Details.ToLower().Contains(term));
        }

        return query;
    }
}
