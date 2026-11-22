using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class CommunicationsService : ICommunicationsService
{
    private readonly List<Broadcast> _broadcasts = [];
    private readonly List<AudienceOption> _audiences = [];
    private readonly List<ChannelHealth> _channelHealth = [];
    private CommunicationDraftDto? _currentDraft;
    private readonly object _lock = new();
    private int _broadcastCounter = 100;

    public CommunicationsService()
    {
        InitializeData();
    }

    private void InitializeData()
    {
        // 1. Initial Audiences
        _audiences.AddRange([
            new AudienceOption { Label = "Volunteers assigned to TASK-782", Count = 18, Category = "Task" },
            new AudienceOption { Label = "All active volunteers", Count = 2184, Category = "All" },
            new AudienceOption { Label = "Available volunteers", Count = 642, Category = "Availability" },
            new AudienceOption { Label = "North Ward volunteers", Count = 142, Category = "Ward" },
            new AudienceOption { Label = "Medical response teams", Count = 58, Category = "Specialist" },
            new AudienceOption { Label = "Logistics teams", Count = 96, Category = "Specialist" },
            new AudienceOption { Label = "Unassigned volunteers", Count = 84, Category = "Availability" },
            new AudienceOption { Label = "Evacuation Support volunteers", Count = 112, Category = "Task" },
            new AudienceOption { Label = "Central Clinic team", Count = 36, Category = "Location" }
        ]);

        // 2. Initial Channel Health
        _channelHealth.AddRange([
            new ChannelHealth
            {
                Channel = "In-app",
                Status = "Operational",
                SuccessRate = 99.8,
                Latency = "0.4s",
                Description = "Operational"
            },
            new ChannelHealth
            {
                Channel = "Email",
                Status = "Operational",
                SuccessRate = 98.6,
                Latency = "1.2s",
                Description = "Operational"
            },
            new ChannelHealth
            {
                Channel = "SMS",
                Status = "Degraded",
                SuccessRate = 92.1,
                Latency = "4.6s",
                Description = "Degraded"
            }
        ]);

        // 3. Initial Broadcasts
        var now = DateTime.UtcNow;

        _broadcasts.AddRange([
            new Broadcast
            {
                Id = "BRD-001",
                Title = "Shift reminder · North Ward",
                Audience = "Volunteers assigned to TASK-782",
                Delivered = 142,
                CreatedAt = now.AddMinutes(-12),
                Time = "12 min ago",
                Channel = "In-app",
                Channels = ["In-app"],
                Status = "Delivered",
                Message = "Please arrive at the assigned location before your scheduled shift.",
                Sender = "Nadia Okafor (Coordinator)"
            },
            new Broadcast
            {
                Id = "BRD-002",
                Title = "Safety update · All teams",
                Audience = "All active volunteers",
                Delivered = 1204,
                CreatedAt = now.AddHours(-2),
                Time = "2 hr ago",
                Channel = "Email",
                Channels = ["Email"],
                Status = "Delivered",
                Message = "Follow the updated field safety instructions during operations. Always wear high-visibility gear.",
                Sender = "Elena Vance (Safety Lead)"
            },
            new Broadcast
            {
                Id = "BRD-003",
                Title = "Task cancellation · TASK-742",
                Audience = "Volunteers assigned to TASK-742",
                Delivered = 8,
                CreatedAt = now.AddHours(-5),
                Time = "5 hr ago",
                Channel = "SMS",
                Channels = ["SMS"],
                Status = "Delivered",
                Message = "The assigned task has been cancelled. Please contact your coordinator for re-assignment.",
                Sender = "Nadia Okafor (Coordinator)"
            },
            new Broadcast
            {
                Id = "BRD-004",
                Title = "Weather Alert: Heavy Monsoon Inbound",
                Audience = "North Ward volunteers",
                Delivered = 142,
                CreatedAt = now.AddHours(-9),
                Time = "9 hr ago",
                Channel = "In-app, SMS",
                Channels = ["In-app", "SMS"],
                Status = "Delivered",
                Message = "Localized heavy showers expected between 15:00 and 19:00. Shelter and water-repellent equipment mandatory.",
                Sender = "Operations Command"
            },
            new Broadcast
            {
                Id = "BRD-005",
                Title = "Supply depot restocking completed",
                Audience = "Logistics teams",
                Delivered = 96,
                CreatedAt = now.AddHours(-18),
                Time = "18 hr ago",
                Channel = "In-app, Email",
                Channels = ["In-app", "Email"],
                Status = "Delivered",
                Message = "Additional medical rations and emergency thermal blankets now available at Central Warehouse Hub 3.",
                Sender = "Supply Dispatch"
            }
        ]);
    }

    public Task<CommunicationsOverviewDto> GetOverviewAsync()
    {
        lock (_lock)
        {
            var totalBroadcasts = _broadcasts.Count;
            var totalDelivered = _broadcasts.Sum(b => b.Delivered);
            var activeVolunteers = _audiences.FirstOrDefault(a => a.Label == "All active volunteers")?.Count ?? 2184;

            var overview = new CommunicationsOverviewDto
            {
                TotalBroadcasts = totalBroadcasts,
                TotalRecipientsDelivered = totalDelivered,
                ActiveVolunteers = activeVolunteers,
                DeliverySuccessRate = 98.7,
                ChannelHealth = [.. _channelHealth]
            };

            return Task.FromResult(overview);
        }
    }

    public Task<List<AudienceOption>> GetAudiencesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_audiences.ToList());
        }
    }

    public Task<List<ChannelHealth>> GetChannelHealthAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_channelHealth.ToList());
        }
    }

    public Task<PaginatedResult<Broadcast>> GetBroadcastsAsync(BroadcastQueryDto query)
    {
        lock (_lock)
        {
            IEnumerable<Broadcast> items = _broadcasts;

            // Recalculate relative time dynamically
            foreach (var b in items)
            {
                b.Time = FormatRelativeTime(b.CreatedAt);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLowerInvariant();
                items = items.Where(b =>
                    b.Title.ToLowerInvariant().Contains(s) ||
                    b.Message.ToLowerInvariant().Contains(s) ||
                    b.Audience.ToLowerInvariant().Contains(s) ||
                    b.Channel.ToLowerInvariant().Contains(s) ||
                    b.Id.ToLowerInvariant().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(query.Channel) && query.Channel != "All")
            {
                var ch = query.Channel.Trim().ToLowerInvariant();
                items = items.Where(b => b.Channels.Any(c => c.Equals(ch, StringComparison.OrdinalIgnoreCase)) ||
                                         b.Channel.ToLowerInvariant().Contains(ch));
            }

            if (!string.IsNullOrWhiteSpace(query.Status) && query.Status != "All")
            {
                var st = query.Status.Trim().ToLowerInvariant();
                items = items.Where(b => b.Status.Equals(st, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Audience) && query.Audience != "All")
            {
                var aud = query.Audience.Trim().ToLowerInvariant();
                items = items.Where(b => b.Audience.ToLowerInvariant().Contains(aud));
            }

            var ordered = items.OrderByDescending(b => b.CreatedAt).ToList();
            var totalCount = ordered.Count;
            var page = Math.Max(1, query.PageNumber);
            var pageSize = query.PageSize > 0 ? query.PageSize : 20;

            var paged = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Task.FromResult(new PaginatedResult<Broadcast>
            {
                Items = paged,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            });
        }
    }

    public Task<Broadcast?> GetBroadcastByIdAsync(string id)
    {
        lock (_lock)
        {
            var broadcast = _broadcasts.FirstOrDefault(b => b.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (broadcast != null)
            {
                broadcast.Time = FormatRelativeTime(broadcast.CreatedAt);
            }
            return Task.FromResult(broadcast);
        }
    }

    public Task<Broadcast> CreateBroadcastAsync(CreateBroadcastRequest request)
    {
        lock (_lock)
        {
            _broadcastCounter++;
            var newId = $"BRD-{_broadcastCounter:D3}";

            // Estimate delivered recipients based on audience
            var matchedAudience = _audiences.FirstOrDefault(a =>
                a.Label.Equals(request.Audience, StringComparison.OrdinalIgnoreCase));
            var recipientsCount = matchedAudience?.Count ?? 45;

            var channels = request.Channels.Count > 0 ? request.Channels : ["In-app"];
            var channelString = string.Join(", ", channels);

            var now = DateTime.UtcNow;

            var broadcast = new Broadcast
            {
                Id = newId,
                Title = request.Title.Trim(),
                Audience = request.Audience.Trim(),
                Delivered = recipientsCount,
                CreatedAt = now,
                Time = "Just now",
                Channels = channels,
                Channel = channelString,
                Status = "Delivered",
                Message = request.Message.Trim(),
                Sender = string.IsNullOrWhiteSpace(request.Sender) ? "Nadia Okafor (Coordinator)" : request.Sender.Trim(),
                FailedCount = 0
            };

            _broadcasts.Insert(0, broadcast);

            // If draft matches this, clear it
            if (_currentDraft != null && _currentDraft.Title == request.Title)
            {
                _currentDraft = null;
            }

            return Task.FromResult(broadcast);
        }
    }

    public Task<bool> DeleteBroadcastAsync(string id)
    {
        lock (_lock)
        {
            var item = _broadcasts.FirstOrDefault(b => b.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null) return Task.FromResult(false);

            _broadcasts.Remove(item);
            return Task.FromResult(true);
        }
    }

    public Task<Broadcast?> ResendBroadcastAsync(string id)
    {
        lock (_lock)
        {
            var item = _broadcasts.FirstOrDefault(b => b.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null) return Task.FromResult<Broadcast?>(null);

            item.CreatedAt = DateTime.UtcNow;
            item.Time = "Just now";
            item.Status = "Delivered";

            // Move to top
            _broadcasts.Remove(item);
            _broadcasts.Insert(0, item);

            return Task.FromResult<Broadcast?>(item);
        }
    }

    public Task<CommunicationDraftDto?> GetDraftAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_currentDraft);
        }
    }

    public Task<CommunicationDraftDto> SaveDraftAsync(SaveDraftRequest request)
    {
        lock (_lock)
        {
            _currentDraft = new CommunicationDraftDto
            {
                Id = "draft-saved",
                Title = request.Title,
                Message = request.Message,
                Audience = request.Audience,
                Channels = request.Channels,
                UpdatedAt = DateTime.UtcNow
            };

            return Task.FromResult(_currentDraft);
        }
    }

    public Task<bool> ClearDraftAsync()
    {
        lock (_lock)
        {
            _currentDraft = null;
            return Task.FromResult(true);
        }
    }

    public Task<byte[]> ExportCsvAsync(BroadcastQueryDto query)
    {
        lock (_lock)
        {
            var items = _broadcasts.OrderByDescending(b => b.CreatedAt).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Broadcast ID,Title,Target Audience,Recipients Delivered,Delivery Channels,Status,Created At,Message");

            foreach (var b in items)
            {
                var cleanTitle = EscapeCsv(b.Title);
                var cleanAudience = EscapeCsv(b.Audience);
                var cleanChannels = EscapeCsv(b.Channel);
                var cleanStatus = EscapeCsv(b.Status);
                var cleanDate = EscapeCsv(b.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"));
                var cleanMessage = EscapeCsv(b.Message);

                sb.AppendLine($"\"{b.Id}\",\"{cleanTitle}\",\"{cleanAudience}\",{b.Delivered},\"{cleanChannels}\",\"{cleanStatus}\",\"{cleanDate}\",\"{cleanMessage}\"");
            }

            var utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var result = new byte[utf8Bom.Length + contentBytes.Length];
            Buffer.BlockCopy(utf8Bom, 0, result, 0, utf8Bom.Length);
            Buffer.BlockCopy(contentBytes, 0, result, utf8Bom.Length, contentBytes.Length);

            return Task.FromResult(result);
        }
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\"", "\"\"");
    }

    private static string FormatRelativeTime(DateTime dateTime)
    {
        var diff = DateTime.UtcNow - dateTime;
        if (diff.TotalSeconds < 60) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hr ago";
        if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
        return dateTime.ToString("MMM dd, yyyy");
    }
}
