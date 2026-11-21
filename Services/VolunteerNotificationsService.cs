using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class VolunteerNotificationsService : IVolunteerNotificationsService
{
    private readonly List<VolunteerNotification> _notifications = [];
    private readonly object _lock = new();
    private VolunteerNotificationPreferencesDto _preferences = new();
    private int _idCounter = 20;

    public VolunteerNotificationsService()
    {
        InitializeSeedData();
    }

    private void InitializeSeedData()
    {
        var now = DateTime.UtcNow;

        _notifications.AddRange([
            new VolunteerNotification
            {
                Id = 1,
                Title = "Urgent task needs two rescue drivers",
                Description = "TASK-782 · North Ward",
                Time = "3 min ago",
                CreatedAt = now.AddMinutes(-3),
                Category = "Safety",
                IconName = "AlertTriangle",
                Color = "bg-[#fdebec] text-[#dc3545]",
                Unread = true,
                Urgent = true,
                Details = "An urgent evacuation transport task requires two additional rescue drivers in North Ward. Please coordinate available volunteers immediately.",
                TargetId = "TASK-782"
            },
            new VolunteerNotification
            {
                Id = 2,
                Title = "4 priority verifications awaiting review",
                Description = "Nursing and heavy vehicle skills",
                Time = "18 min ago",
                CreatedAt = now.AddMinutes(-18),
                Category = "Verification",
                IconName = "ShieldCheck",
                Color = "bg-[#fff3df] text-[#b87813]",
                Unread = true,
                Urgent = false,
                Details = "Four volunteer verification applications require coordinator review. These include nursing qualifications and heavy vehicle driving certifications.",
                TargetId = "VER-401"
            },
            new VolunteerNotification
            {
                Id = 3,
                Title = "AI match ready for approval",
                Description = "TASK-774 · 6 recommended volunteers",
                Time = "31 min ago",
                CreatedAt = now.AddMinutes(-31),
                Category = "AI Matching",
                IconName = "Sparkles",
                Color = "bg-[#f0eafb] text-[#7953c6]",
                Unread = true,
                Urgent = false,
                Details = "The AI matching service has identified six suitable volunteers for TASK-774. Review the recommendations and approve or reject the proposed matches.",
                TargetId = "TASK-774"
            },
            new VolunteerNotification
            {
                Id = 4,
                Title = "Volunteer team checked in",
                Description = "TASK-779 · Central Clinic",
                Time = "1 hr ago",
                CreatedAt = now.AddHours(-1),
                Category = "Assignments",
                IconName = "Check",
                Color = "bg-[#e5f5ef] text-[#239879]",
                Unread = false,
                Urgent = false,
                Details = "The assigned volunteer team has successfully checked in at Central Clinic. Their field activity status is now active.",
                TargetId = "TASK-779"
            },
            new VolunteerNotification
            {
                Id = 5,
                Title = "42 certifications expiring soon",
                Description = "Review within 30 days",
                Time = "2 hr ago",
                CreatedAt = now.AddHours(-2),
                Category = "Verification",
                IconName = "Award",
                Color = "bg-[#fff3df] text-[#b87813]",
                Unread = false,
                Urgent = false,
                Details = "There are 42 volunteer certifications scheduled to expire within the next 30 days. Review the certification attention queue and contact affected volunteers.",
                TargetId = "EXP-30"
            },
            new VolunteerNotification
            {
                Id = 6,
                Title = "Broadcast delivery completed",
                Description = "1,204 of 1,204 recipients",
                Time = "3 hr ago",
                CreatedAt = now.AddHours(-3),
                Category = "Communications",
                IconName = "MessageSquare",
                Color = "bg-[#e8f1f6] text-[#287fa9]",
                Unread = false,
                Urgent = false,
                Details = "The latest operational safety update was successfully delivered to all 1,204 intended recipients.",
                TargetId = "BRD-002"
            },
            new VolunteerNotification
            {
                Id = 7,
                Title = "New volunteer assignment accepted",
                Description = "ASGN-4421 · Elena Rossi",
                Time = "4 hr ago",
                CreatedAt = now.AddHours(-4),
                Category = "Assignments",
                IconName = "Users",
                Color = "bg-[#e5f5ef] text-[#239879]",
                Unread = true,
                Urgent = false,
                Details = "Elena Rossi has accepted the evacuation support assignment. The volunteer is scheduled to report to North Ward.",
                TargetId = "ASGN-4421"
            },
            new VolunteerNotification
            {
                Id = 8,
                Title = "Safety incident reported",
                Description = "West District · Field alert",
                Time = "5 hr ago",
                CreatedAt = now.AddHours(-5),
                Category = "Safety",
                IconName = "AlertTriangle",
                Color = "bg-[#fdebec] text-[#dc3545]",
                Unread = true,
                Urgent = true,
                Details = "A field safety incident has been reported in West District. Review the incident information and coordinate an appropriate response.",
                TargetId = "INC-109"
            },
            new VolunteerNotification
            {
                Id = 9,
                Title = "Volunteer verification approved",
                Description = "Maya Fernando · First Aid",
                Time = "6 hr ago",
                CreatedAt = now.AddHours(-6),
                Category = "Verification",
                IconName = "ShieldCheck",
                Color = "bg-[#e5f5ef] text-[#239879]",
                Unread = false,
                Urgent = false,
                Details = "Maya Fernando's First Aid qualification has been reviewed and approved.",
                TargetId = "VER-392"
            },
            new VolunteerNotification
            {
                Id = 10,
                Title = "AI matching model updated",
                Description = "Model version 2.4",
                Time = "Yesterday",
                CreatedAt = now.AddDays(-1),
                Category = "AI Matching",
                IconName = "Sparkles",
                Color = "bg-[#f0eafb] text-[#7953c6]",
                Unread = false,
                Urgent = false,
                Details = "The volunteer matching model has been updated to version 2.4 with improvements to suitability scoring.",
                TargetId = "MDL-24"
            }
        ]);
    }

    public Task<VolunteerNotificationSummaryDto> GetSummaryAsync()
    {
        lock (_lock)
        {
            var summary = new VolunteerNotificationSummaryDto
            {
                TotalCount = _notifications.Count,
                UnreadCount = _notifications.Count(n => n.Unread),
                UrgentCount = _notifications.Count(n => n.Urgent && n.Unread),
                AssignmentCount = _notifications.Count(n => n.Category == "Assignments"),
                VerificationCount = _notifications.Count(n => n.Category == "Verification"),
                SafetyCount = _notifications.Count(n => n.Category == "Safety"),
                AiMatchingCount = _notifications.Count(n => n.Category == "AI Matching"),
                CommunicationsCount = _notifications.Count(n => n.Category == "Communications"),
                DeliveryServices =
                [
                    new DeliveryServiceStatusDto
                    {
                        Name = "In-app notifications",
                        Status = "Operational",
                        Message = "In-app operational"
                    },
                    new DeliveryServiceStatusDto
                    {
                        Name = "Email notifications",
                        Status = "Operational",
                        Message = "Email operational"
                    },
                    new DeliveryServiceStatusDto
                    {
                        Name = "SMS notifications",
                        Status = "Degraded",
                        Message = "SMS provider degraded"
                    }
                ]
            };

            return Task.FromResult(summary);
        }
    }

    public Task<PaginatedResult<VolunteerNotification>> GetNotificationsAsync(VolunteerNotificationQueryDto query)
    {
        lock (_lock)
        {
            IEnumerable<VolunteerNotification> items = _notifications;

            // Recalculate relative time dynamically
            foreach (var n in items)
            {
                n.Time = FormatRelativeTime(n.CreatedAt);
            }

            if (!string.IsNullOrWhiteSpace(query.Tab) && query.Tab != "All")
            {
                if (query.Tab.Equals("Unread", StringComparison.OrdinalIgnoreCase))
                {
                    items = items.Where(n => n.Unread);
                }
                else
                {
                    items = items.Where(n => n.Category.Equals(query.Tab, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (query.UnreadOnly.HasValue && query.UnreadOnly.Value)
            {
                items = items.Where(n => n.Unread);
            }

            if (query.UrgentOnly.HasValue && query.UrgentOnly.Value)
            {
                items = items.Where(n => n.Urgent);
            }

            if (!string.IsNullOrWhiteSpace(query.Category) && query.Category != "All")
            {
                items = items.Where(n => n.Category.Equals(query.Category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLowerInvariant();
                items = items.Where(n =>
                    n.Title.ToLowerInvariant().Contains(s) ||
                    n.Description.ToLowerInvariant().Contains(s) ||
                    n.Category.ToLowerInvariant().Contains(s) ||
                    n.Details.ToLowerInvariant().Contains(s) ||
                    (n.TargetId != null && n.TargetId.ToLowerInvariant().Contains(s)));
            }

            var ordered = items.OrderByDescending(n => n.CreatedAt).ToList();
            var totalCount = ordered.Count;
            var page = Math.Max(1, query.PageNumber);
            var pageSize = query.PageSize > 0 ? query.PageSize : 6;

            var paged = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Task.FromResult(new PaginatedResult<VolunteerNotification>
            {
                Items = paged,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            });
        }
    }

    public Task<VolunteerNotification?> GetByIdAsync(int id)
    {
        lock (_lock)
        {
            var item = _notifications.FirstOrDefault(n => n.Id == id);
            if (item != null)
            {
                item.Time = FormatRelativeTime(item.CreatedAt);
            }
            return Task.FromResult(item);
        }
    }

    public Task<bool> MarkAsReadAsync(int id)
    {
        lock (_lock)
        {
            var item = _notifications.FirstOrDefault(n => n.Id == id);
            if (item == null) return Task.FromResult(false);

            item.Unread = false;
            return Task.FromResult(true);
        }
    }

    public Task<int> MarkAllAsReadAsync()
    {
        lock (_lock)
        {
            var unread = _notifications.Where(n => n.Unread).ToList();
            foreach (var n in unread)
            {
                n.Unread = false;
            }
            return Task.FromResult(unread.Count);
        }
    }

    public Task<bool> DeleteNotificationAsync(int id)
    {
        lock (_lock)
        {
            var item = _notifications.FirstOrDefault(n => n.Id == id);
            if (item == null) return Task.FromResult(false);

            _notifications.Remove(item);
            return Task.FromResult(true);
        }
    }

    public Task<VolunteerNotification> CreateNotificationAsync(CreateVolunteerNotificationRequest request)
    {
        lock (_lock)
        {
            _idCounter++;

            var icon = !string.IsNullOrWhiteSpace(request.IconName) ? request.IconName : GetDefaultIcon(request.Category, request.Urgent);
            var color = !string.IsNullOrWhiteSpace(request.Color) ? request.Color : GetDefaultColor(request.Category, request.Urgent);

            var item = new VolunteerNotification
            {
                Id = _idCounter,
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Category = request.Category.Trim(),
                Details = request.Details.Trim(),
                Urgent = request.Urgent,
                Unread = true,
                CreatedAt = DateTime.UtcNow,
                Time = "Just now",
                TargetId = request.TargetId?.Trim(),
                IconName = icon,
                Color = color
            };

            _notifications.Insert(0, item);
            return Task.FromResult(item);
        }
    }

    public Task<VolunteerNotificationPreferencesDto> GetPreferencesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_preferences);
        }
    }

    public Task<VolunteerNotificationPreferencesDto> UpdatePreferencesAsync(VolunteerNotificationPreferencesDto preferences)
    {
        lock (_lock)
        {
            _preferences = preferences;
            return Task.FromResult(_preferences);
        }
    }

    public Task<byte[]> ExportCsvAsync(VolunteerNotificationQueryDto query)
    {
        lock (_lock)
        {
            var items = _notifications.OrderByDescending(n => n.CreatedAt).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("ID,Title,Description,Category,Urgent,Unread,Created At,Target ID,Details");

            foreach (var n in items)
            {
                var cleanTitle = EscapeCsv(n.Title);
                var cleanDesc = EscapeCsv(n.Description);
                var cleanCat = EscapeCsv(n.Category);
                var cleanTarget = EscapeCsv(n.TargetId ?? "");
                var cleanDetails = EscapeCsv(n.Details);
                var cleanDate = n.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC");

                sb.AppendLine($"{n.Id},\"{cleanTitle}\",\"{cleanDesc}\",\"{cleanCat}\",{n.Urgent},{n.Unread},\"{cleanDate}\",\"{cleanTarget}\",\"{cleanDetails}\"");
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

    private static string GetDefaultIcon(string category, bool urgent)
    {
        if (urgent) return "AlertTriangle";
        return category.ToLowerInvariant() switch
        {
            "verification" => "ShieldCheck",
            "ai matching" => "Sparkles",
            "communications" => "MessageSquare",
            "safety" => "AlertTriangle",
            _ => "Check"
        };
    }

    private static string GetDefaultColor(string category, bool urgent)
    {
        if (urgent) return "bg-[#fdebec] text-[#dc3545]";
        return category.ToLowerInvariant() switch
        {
            "verification" => "bg-[#fff3df] text-[#b87813]",
            "ai matching" => "bg-[#f0eafb] text-[#7953c6]",
            "communications" => "bg-[#e8f1f6] text-[#287fa9]",
            "safety" => "bg-[#fdebec] text-[#dc3545]",
            _ => "bg-[#e5f5ef] text-[#239879]"
        };
    }
}
