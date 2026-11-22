using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using CareLink.Core.Models;
using CareLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly CareLinkDbContext _context;
    private NotificationPreferences _safnaPreferences = new();
    private readonly object _prefLock = new();

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

    public async Task<IReadOnlyList<NotificationItem>> GetAllAsync(
        NotificationFilterParams? filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications.AsNoTracking().AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim().ToLower();
                query = query.Where(n =>
                    n.Title.ToLower().Contains(s) ||
                    n.Description.ToLower().Contains(s) ||
                    n.Reference.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(filter.Category) &&
                !filter.Category.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var c = filter.Category.Trim().ToLower();
                query = query.Where(n => n.Type.ToLower() == c);
            }

            if (filter.UnreadOnly == true)
            {
                query = query.Where(n => n.Unread);
            }

            if (filter.UrgentOnly == true)
            {
                query = query.Where(n => n.Urgent);
            }

            if (!string.IsNullOrWhiteSpace(filter.Role) &&
                !filter.Role.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                var r = filter.Role.Trim().ToLower();
                query = query.Where(n => n.Role.ToLower() == r || n.Role.ToLower() == "all");
            }
        }

        return await query.OrderByDescending(n => n.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(
        NotificationFilterParams? filter = null,
        CancellationToken cancellationToken = default)
    {
        var list = await GetAllAsync(filter, cancellationToken);
        return list.Count;
    }

    public async Task<NotificationItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<NotificationItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (int.TryParse(id, out var intId))
        {
            var found = await GetByIdAsync(intId, cancellationToken);
            if (found != null) return found;
        }

        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Reference == id, cancellationToken);
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
        existing.ReadAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MarkReadAsync(string id, CancellationToken cancellationToken = default)
    {
        if (int.TryParse(id, out var intId))
        {
            return await MarkAsReadAsync(intId, cancellationToken);
        }

        var existing = await _context.Notifications.FirstOrDefaultAsync(n => n.Reference == id, cancellationToken);
        if (existing == null) return false;

        existing.Unread = false;
        existing.ReadAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications.Where(n => n.Unread);
        if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.Role == role || n.Role == "All" || n.Role == "all");
        }

        var unreadItems = await query.ToListAsync(cancellationToken);
        if (unreadItems.Count == 0) return 0;

        foreach (var item in unreadItems)
        {
            item.Unread = false;
            item.ReadAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return unreadItems.Count;
    }

    public async Task<int> MarkAllReadAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        return await MarkAllAsReadAsync(role, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        if (existing == null) return false;

        _context.Notifications.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (int.TryParse(id, out var intId))
        {
            return await DeleteAsync(intId, cancellationToken);
        }

        var existing = await _context.Notifications.FirstOrDefaultAsync(n => n.Reference == id, cancellationToken);
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
            query = query.Where(n => n.Role == role || n.Role == "All" || n.Role == "all");
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetTypeCountsAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.Role == role || n.Role == "All" || n.Role == "all");
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

    public Task<NotificationPreferences> GetPreferencesAsync(CancellationToken cancellationToken = default)
    {
        lock (_prefLock)
        {
            return Task.FromResult(_safnaPreferences);
        }
    }

    public Task<NotificationPreferences> UpdatePreferencesAsync(NotificationPreferences preferences, CancellationToken cancellationToken = default)
    {
        lock (_prefLock)
        {
            _safnaPreferences = preferences;
            return Task.FromResult(_safnaPreferences);
        }
    }

    public async Task<NotificationMetrics> GetMetricsAsync(string? role = null, CancellationToken cancellationToken = default)
    {
        var all = await _context.Notifications.AsNoTracking().ToListAsync(cancellationToken);

        return new NotificationMetrics
        {
            All = all.Count,
            Unread = all.Count(n => n.Unread),
            Urgent = all.Count(n => n.Urgent),
            Critical = all.Count(n => n.Type.Equals("Critical", StringComparison.OrdinalIgnoreCase)),
            Ai = all.Count(n => n.Type.Equals("ai", StringComparison.OrdinalIgnoreCase)),
            Resource = all.Count(n => n.Type.Equals("Inventory", StringComparison.OrdinalIgnoreCase) || n.Type.Equals("Resource", StringComparison.OrdinalIgnoreCase)),
            Shelter = all.Count(n => n.Type.Equals("Requests", StringComparison.OrdinalIgnoreCase) || n.Type.Equals("Shelter", StringComparison.OrdinalIgnoreCase)),
            Partner = all.Count(n => n.Role.Equals("Partner", StringComparison.OrdinalIgnoreCase)),
            Report = all.Count(n => n.Type.Equals("Reports", StringComparison.OrdinalIgnoreCase)),
            Collaboration = all.Count(n => n.Type.Equals("Collaboration", StringComparison.OrdinalIgnoreCase)),
            Reporting = all.Count(n => n.Type.Equals("Reporting", StringComparison.OrdinalIgnoreCase)),
            Team = all.Count(n => n.Type.Equals("Team", StringComparison.OrdinalIgnoreCase)),
            Agreement = all.Count(n => n.Type.Equals("Agreement", StringComparison.OrdinalIgnoreCase)),
            LastUpdated = DateTimeOffset.UtcNow
        };
    }

    public async Task ResetDefaultDataAsync(CancellationToken cancellationToken = default)
    {
        _context.Notifications.RemoveRange(_context.Notifications);
        await _context.SaveChangesAsync(cancellationToken);
        await DataSeeder.SeedAsync(_context);
    }

    private IQueryable<NotificationItem> BuildQuery(NotificationQueryParameters parameters)
    {
        var query = _context.Notifications.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Role) &&
            !parameters.Role.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.Role == parameters.Role || n.Role == "All" || n.Role == "all");
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
