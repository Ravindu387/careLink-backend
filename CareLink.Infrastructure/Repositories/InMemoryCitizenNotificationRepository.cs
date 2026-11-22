namespace CareLink.Infrastructure.Repositories;

using System.Collections.Concurrent;
using CareLink.Application.Interfaces;
using CareLink.Core.Entities;

public class InMemoryCitizenNotificationRepository : ICitizenNotificationRepository
{
    private readonly ConcurrentDictionary<string, CitizenNotification> _notifications = new();
    private readonly Lock _lock = new();

    public InMemoryCitizenNotificationRepository()
    {
        SeedInitialData();
    }

    public Task<IEnumerable<CitizenNotification>> GetAllAsync(
        string? citizenName = null,
        string? category = null,
        bool? unreadOnly = null,
        string? search = null)
    {
        var query = _notifications.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(citizenName))
        {
            query = query.Where(n => n.CitizenName.Equals(citizenName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.Category.Equals(category.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (unreadOnly == true)
        {
            query = query.Where(n => n.Unread);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(n =>
                n.Title.ToLowerInvariant().Contains(s) ||
                n.Description.ToLowerInvariant().Contains(s) ||
                n.Category.ToLowerInvariant().Contains(s) ||
                n.Type.ToLowerInvariant().Contains(s)
            );
        }

        var sorted = query.OrderByDescending(n => n.CreatedAt).ToList();
        return Task.FromResult<IEnumerable<CitizenNotification>>(sorted);
    }

    public Task<CitizenNotification?> GetByIdAsync(string id)
    {
        _notifications.TryGetValue(id, out var notif);
        return Task.FromResult(notif);
    }

    public Task<CitizenNotification> CreateAsync(CitizenNotification notification)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(notification.Id))
            {
                notification.Id = (_notifications.Count + 1).ToString();
            }

            if (notification.CreatedAt == default)
            {
                notification.CreatedAt = DateTime.UtcNow;
            }

            _notifications[notification.Id] = notification;
            return Task.FromResult(notification);
        }
    }

    public Task<bool> MarkAsReadAsync(string id)
    {
        if (_notifications.TryGetValue(id, out var notif))
        {
            notif.Unread = false;
            notif.ReadAt = DateTime.UtcNow;
            _notifications[id] = notif;
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task<int> MarkAllAsReadAsync(string? citizenName = null)
    {
        lock (_lock)
        {
            var count = 0;
            var targetCitizen = string.IsNullOrWhiteSpace(citizenName) ? "Samira Khan" : citizenName.Trim();

            foreach (var kvp in _notifications)
            {
                if (kvp.Value.CitizenName.Equals(targetCitizen, StringComparison.OrdinalIgnoreCase) && kvp.Value.Unread)
                {
                    kvp.Value.Unread = false;
                    kvp.Value.ReadAt = DateTime.UtcNow;
                    count++;
                }
            }

            return Task.FromResult(count);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        return Task.FromResult(_notifications.TryRemove(id, out _));
    }

    private void SeedInitialData()
    {
        var list = new List<CitizenNotification>
        {
            new()
            {
                Id = "1",
                Type = "alert",
                Title = "Evacuation alert for Riverside",
                Description = "Move to North Ward Community Hall using Market Road.",
                Category = "Safety alerts",
                Unread = true,
                ActionUrl = "/citizen/safety-updates",
                CitizenName = "Samira Khan",
                CitizenId = "CIT-001",
                CreatedAt = DateTime.UtcNow.AddMinutes(-12)
            },
            new()
            {
                Id = "2",
                Type = "message",
                Title = "New message from Case Support",
                Description = "Accessible shelter space is being confirmed.",
                Category = "My requests",
                Unread = true,
                ActionUrl = "/citizen/messages",
                CitizenName = "Samira Khan",
                CitizenId = "CIT-001",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new()
            {
                Id = "3",
                Type = "delivery",
                Title = "Your delivery is on the way",
                Description = "Food and water delivery expected by 14:20.",
                Category = "My requests",
                Unread = true,
                ActionUrl = "/citizen/my-requests",
                CitizenName = "Samira Khan",
                CitizenId = "CIT-001",
                CreatedAt = DateTime.UtcNow.AddMinutes(-42)
            },
            new()
            {
                Id = "4",
                Type = "shelter",
                Title = "Shelter availability updated",
                Description = "Riverton High School has 48 spaces available.",
                Category = "Safety alerts",
                Unread = true,
                ActionUrl = "/citizen/shelters",
                CitizenName = "Samira Khan",
                CitizenId = "CIT-001",
                CreatedAt = DateTime.UtcNow.AddHours(-1).AddMinutes(-5)
            },
            new()
            {
                Id = "5",
                Type = "medical",
                Title = "Medical request completed",
                Description = "Support delivery was confirmed.",
                Category = "My requests",
                Unread = false,
                ActionUrl = "/citizen/my-requests",
                CitizenName = "Samira Khan",
                CitizenId = "CIT-001",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ReadAt = DateTime.UtcNow.AddHours(-12)
            }
        };

        foreach (var item in list)
        {
            _notifications[item.Id] = item;
        }
    }
}
