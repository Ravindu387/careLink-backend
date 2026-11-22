using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class VolunteerVerificationsService : IVolunteerVerificationsService
{
    private readonly List<VolunteerVerificationApplication> _applications = [];
    private readonly object _lock = new();
    private int _counter = 220;

    public VolunteerVerificationsService()
    {
        InitializeSeedData();
    }

    private void InitializeSeedData()
    {
        var now = DateTime.UtcNow;

        _applications.AddRange([
            new VolunteerVerificationApplication
            {
                Id = "VER-218",
                Applicant = "Sara Khan",
                Skills = ["Shelter Support", "Childcare"],
                Location = "Central Ward",
                Documents = ["ID", "First Aid Certificate"],
                Submitted = "2 hours ago",
                CreatedAt = now.AddHours(-2),
                Priority = "Standard",
                DocumentState = "Complete",
                Status = "Pending",
                Email = "sara@example.com",
                Phone = "+94 71 234 5678"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-215",
                Applicant = "Abdul Rahman",
                Skills = ["Heavy Vehicle", "Logistics"],
                Location = "North Ward",
                Documents = ["ID", "Driving Licence"],
                Submitted = "5 hours ago",
                CreatedAt = now.AddHours(-5),
                Priority = "High",
                DocumentState = "Complete",
                Status = "Pending",
                Email = "abdul@example.com",
                Phone = "+94 72 345 6789"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-209",
                Applicant = "Maya Fernando",
                Skills = ["Nursing", "First Aid"],
                Location = "Central Ward",
                Documents = ["ID", "Nursing Registration"],
                Submitted = "Yesterday",
                CreatedAt = now.AddDays(-1),
                Priority = "High",
                DocumentState = "Complete",
                Status = "Pending",
                Email = "maya@example.com",
                Phone = "+94 76 456 7890"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-204",
                Applicant = "Theo Martin",
                Skills = ["Search & Rescue"],
                Location = "West District",
                Documents = ["ID", "Rescue Certificate"],
                Submitted = "Yesterday",
                CreatedAt = now.AddDays(-1).AddHours(-3),
                Priority = "Standard",
                DocumentState = "Complete",
                Status = "Pending",
                Email = "theo@example.com",
                Phone = "+94 77 567 8901"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-198",
                Applicant = "Daniel Okafor",
                Skills = ["Logistics", "Warehousing"],
                Location = "Eastbank",
                Documents = ["ID"],
                Submitted = "2 days ago",
                CreatedAt = now.AddDays(-2),
                Priority = "Urgent",
                DocumentState = "Incomplete",
                Status = "Pending",
                Email = "daniel@example.com",
                Phone = "+94 75 678 9012"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-192",
                Applicant = "Amara Silva",
                Skills = ["Medical", "First Aid"],
                Location = "South Ward",
                Documents = ["ID", "Medical Certificate"],
                Submitted = "3 days ago",
                CreatedAt = now.AddDays(-3),
                Priority = "Standard",
                DocumentState = "Complete",
                Status = "Pending",
                Email = "amara@example.com",
                Phone = "+94 70 789 0123"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-185",
                Applicant = "James Carter",
                Skills = ["Driving", "Rescue"],
                Location = "North Ward",
                Documents = ["ID", "Driving Licence"],
                Submitted = "3 days ago",
                CreatedAt = now.AddDays(-3).AddHours(-4),
                Priority = "High",
                DocumentState = "Complete",
                Status = "Pending",
                Email = "james@example.com",
                Phone = "+94 78 890 1234"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-178",
                Applicant = "Priya Perera",
                Skills = ["Childcare", "Shelter Support"],
                Location = "Central Ward",
                Documents = ["ID"],
                Submitted = "4 days ago",
                CreatedAt = now.AddDays(-4),
                Priority = "Standard",
                DocumentState = "Incomplete",
                Status = "Pending",
                Email = "priya@example.com",
                Phone = "+94 74 901 2345"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-174",
                Applicant = "Lucas Vance",
                Skills = ["Search & Rescue", "Medical"],
                Location = "West District",
                Documents = ["ID", "Rescue Certification", "CPR Badge"],
                Submitted = "5 hours ago",
                CreatedAt = now.AddHours(-5),
                Priority = "Urgent",
                DocumentState = "Complete",
                Status = "Pending",
                Email = "lucas.vance@example.com",
                Phone = "+94 71 888 1234"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-169",
                Applicant = "Fatima Al-Mansoor",
                Skills = ["First Aid", "Translation"],
                Location = "North Ward",
                Documents = ["ID", "Red Cross Certificate"],
                Submitted = "1 day ago",
                CreatedAt = now.AddDays(-1),
                Priority = "High",
                DocumentState = "Complete",
                Status = "Pending",
                Email = "fatima@example.com",
                Phone = "+94 77 999 4321"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-163",
                Applicant = "Chen Wei",
                Skills = ["Electrical Repair", "Logistics"],
                Location = "Eastbank",
                Documents = ["ID", "Electrician License"],
                Submitted = "2 days ago",
                CreatedAt = now.AddDays(-2),
                Priority = "Standard",
                DocumentState = "Complete",
                Status = "Pending",
                Email = "chen.wei@example.com",
                Phone = "+94 76 111 2233"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-158",
                Applicant = "Marcus Brody",
                Skills = ["Heavy Vehicle", "Driving"],
                Location = "North Ward",
                Documents = ["ID", "Commercial Driver License"],
                Submitted = "6 hours ago",
                CreatedAt = now.AddHours(-6),
                Priority = "Urgent",
                DocumentState = "Complete",
                Status = "Pending",
                Email = "marcus@example.com",
                Phone = "+94 75 333 4455"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-152",
                Applicant = "Ananya Patel",
                Skills = ["Medical", "Nursing"],
                Location = "Central Clinic",
                Documents = ["ID", "Nursing Degree", "Hospital ID"],
                Submitted = "1 day ago",
                CreatedAt = now.AddDays(-1),
                Priority = "High",
                DocumentState = "Complete",
                Status = "Pending",
                Email = "ananya.patel@example.com",
                Phone = "+94 72 555 6677"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-147",
                Applicant = "David Kim",
                Skills = ["Telecommunications", "IT Support"],
                Location = "South Ward",
                Documents = ["ID", "Radio Operator Certificate"],
                Submitted = "2 days ago",
                CreatedAt = now.AddDays(-2),
                Priority = "Standard",
                DocumentState = "Complete",
                Status = "Pending",
                Email = "david.kim@example.com",
                Phone = "+94 70 888 9900"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-141",
                Applicant = "Tariq Aziz",
                Skills = ["Warehousing", "Logistics"],
                Location = "Eastbank",
                Documents = ["ID"],
                Submitted = "3 days ago",
                CreatedAt = now.AddDays(-3),
                Priority = "Standard",
                DocumentState = "Incomplete",
                Status = "Pending",
                Email = "tariq.aziz@example.com",
                Phone = "+94 78 444 7788"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-135",
                Applicant = "Elena Rostova",
                Skills = ["Shelter Support", "Childcare"],
                Location = "Central Ward",
                Documents = ["ID", "Food Handler Card"],
                Submitted = "2 days ago",
                CreatedAt = now.AddDays(-2),
                Priority = "Standard",
                DocumentState = "Complete",
                Status = "Pending",
                Email = "elena.r@example.com",
                Phone = "+94 73 222 8899"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-129",
                Applicant = "Gabriel Santos",
                Skills = ["Water Purification", "Rescue"],
                Location = "West District",
                Documents = ["ID"],
                Submitted = "2 days ago",
                CreatedAt = now.AddDays(-2),
                Priority = "Urgent",
                DocumentState = "Incomplete",
                Status = "Pending",
                Email = "gabriel.s@example.com",
                Phone = "+94 79 123 9988"
            },
            new VolunteerVerificationApplication
            {
                Id = "VER-122",
                Applicant = "Samantha Hayes",
                Skills = ["Childcare", "First Aid"],
                Location = "South Ward",
                Documents = ["ID", "Child Safeguarding Clearance"],
                Submitted = "3 days ago",
                CreatedAt = now.AddDays(-3),
                Priority = "Standard",
                DocumentState = "Complete",
                Status = "Pending",
                Email = "samantha.h@example.com",
                Phone = "+94 74 666 3322"
            }
        ]);
    }

    public Task<VolunteerVerificationStatsDto> GetStatsAsync()
    {
        lock (_lock)
        {
            var pending = _applications.Where(a => a.Status == "Pending").ToList();

            var stats = new VolunteerVerificationStatsDto
            {
                TotalApplications = _applications.Count,
                PendingCount = pending.Count,
                UrgentCount = pending.Count(a => a.Priority == "Urgent"),
                HighPriorityCount = pending.Count(a => a.Priority == "High"),
                StandardPriorityCount = pending.Count(a => a.Priority == "Standard"),
                CompleteDocumentsCount = pending.Count(a => a.DocumentState == "Complete"),
                IncompleteDocumentsCount = pending.Count(a => a.DocumentState == "Incomplete"),
                ApprovedCount = _applications.Count(a => a.Status == "Approved"),
                RejectedCount = _applications.Count(a => a.Status == "Rejected"),
                OldestPendingSubmission = "2 days"
            };

            return Task.FromResult(stats);
        }
    }

    public Task<List<VolunteerVerificationApplication>> GetPendingApplicationsAsync()
    {
        lock (_lock)
        {
            var pending = _applications
                .Where(a => a.Status == "Pending")
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            foreach (var app in pending)
            {
                app.Submitted = FormatRelativeTime(app.CreatedAt);
            }

            return Task.FromResult(pending);
        }
    }

    public Task<PaginatedResult<VolunteerVerificationApplication>> GetApplicationsAsync(VolunteerVerificationQueryDto query)
    {
        lock (_lock)
        {
            IEnumerable<VolunteerVerificationApplication> items = _applications;

            // Recalculate relative submitted time
            foreach (var app in items)
            {
                app.Submitted = FormatRelativeTime(app.CreatedAt);
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(query.Status) && query.Status != "All")
            {
                items = items.Where(a => a.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
            }

            // Priority filter
            if (!string.IsNullOrWhiteSpace(query.Priority) && query.Priority != "All priorities")
            {
                items = items.Where(a => a.Priority.Equals(query.Priority, StringComparison.OrdinalIgnoreCase));
            }

            // Skill filter
            if (!string.IsNullOrWhiteSpace(query.Skill) && query.Skill != "All skill groups")
            {
                items = items.Where(a => a.Skills.Any(s => s.Equals(query.Skill, StringComparison.OrdinalIgnoreCase)));
            }

            // Document state filter
            if (!string.IsNullOrWhiteSpace(query.DocumentState) && query.DocumentState != "All document states")
            {
                items = items.Where(a => a.DocumentState.Equals(query.DocumentState, StringComparison.OrdinalIgnoreCase));
            }

            // Date filter
            if (!string.IsNullOrWhiteSpace(query.DateFilter) && query.DateFilter != "Date submitted")
            {
                var now = DateTime.UtcNow;
                if (query.DateFilter.Equals("Today", StringComparison.OrdinalIgnoreCase))
                {
                    items = items.Where(a => (now - a.CreatedAt).TotalHours <= 24);
                }
                else if (query.DateFilter.Equals("Last 2 days", StringComparison.OrdinalIgnoreCase))
                {
                    items = items.Where(a => (now - a.CreatedAt).TotalDays <= 2);
                }
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLowerInvariant();
                items = items.Where(a =>
                    a.Id.ToLowerInvariant().Contains(s) ||
                    a.Applicant.ToLowerInvariant().Contains(s) ||
                    a.Location.ToLowerInvariant().Contains(s) ||
                    a.Email.ToLowerInvariant().Contains(s) ||
                    a.Skills.Any(sk => sk.ToLowerInvariant().Contains(s)) ||
                    a.Documents.Any(d => d.ToLowerInvariant().Contains(s)));
            }

            var ordered = items.OrderByDescending(a => a.CreatedAt).ToList();
            var totalCount = ordered.Count;
            var page = Math.Max(1, query.PageNumber);
            var pageSize = query.PageSize > 0 ? query.PageSize : 4;

            var paged = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Task.FromResult(new PaginatedResult<VolunteerVerificationApplication>
            {
                Items = paged,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            });
        }
    }

    public Task<VolunteerVerificationApplication?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            var item = _applications.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.Submitted = FormatRelativeTime(item.CreatedAt);
            }
            return Task.FromResult(item);
        }
    }

    public Task<VolunteerVerificationApplication?> ApproveAsync(string id, ReviewVerificationRequest? request = null)
    {
        lock (_lock)
        {
            var item = _applications.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null) return Task.FromResult<VolunteerVerificationApplication?>(null);

            item.Status = "Approved";
            item.ReviewedAt = DateTime.UtcNow;
            item.ReviewedBy = string.IsNullOrWhiteSpace(request?.ReviewedBy) ? "Nadia Okafor (Coordinator)" : request.ReviewedBy.Trim();
            if (!string.IsNullOrWhiteSpace(request?.Notes))
            {
                item.ReviewNotes = request.Notes.Trim();
            }

            return Task.FromResult<VolunteerVerificationApplication?>(item);
        }
    }

    public Task<VolunteerVerificationApplication?> RejectAsync(string id, ReviewVerificationRequest? request = null)
    {
        lock (_lock)
        {
            var item = _applications.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null) return Task.FromResult<VolunteerVerificationApplication?>(null);

            item.Status = "Rejected";
            item.ReviewedAt = DateTime.UtcNow;
            item.ReviewedBy = string.IsNullOrWhiteSpace(request?.ReviewedBy) ? "Nadia Okafor (Coordinator)" : request.ReviewedBy.Trim();
            if (!string.IsNullOrWhiteSpace(request?.Notes))
            {
                item.ReviewNotes = request.Notes.Trim();
            }

            return Task.FromResult<VolunteerVerificationApplication?>(item);
        }
    }

    public Task<VolunteerVerificationApplication> CreateAsync(CreateVerificationApplicationRequest request)
    {
        lock (_lock)
        {
            _counter++;
            var newId = $"VER-{_counter}";

            var now = DateTime.UtcNow;

            var application = new VolunteerVerificationApplication
            {
                Id = newId,
                Applicant = request.Applicant.Trim(),
                Skills = request.Skills.Count > 0 ? request.Skills : ["Volunteer Support"],
                Location = string.IsNullOrWhiteSpace(request.Location) ? "Central Ward" : request.Location.Trim(),
                Documents = request.Documents.Count > 0 ? request.Documents : ["ID"],
                Submitted = "Just now",
                CreatedAt = now,
                Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Standard" : request.Priority.Trim(),
                DocumentState = string.IsNullOrWhiteSpace(request.DocumentState) ? "Complete" : request.DocumentState.Trim(),
                Status = "Pending",
                Email = request.Email.Trim(),
                Phone = request.Phone.Trim()
            };

            _applications.Insert(0, application);
            return Task.FromResult(application);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            var item = _applications.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (item == null) return Task.FromResult(false);

            _applications.Remove(item);
            return Task.FromResult(true);
        }
    }

    public Task<byte[]> ExportCsvAsync(VolunteerVerificationQueryDto query)
    {
        lock (_lock)
        {
            var items = _applications.OrderByDescending(a => a.CreatedAt).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Verification ID,Applicant,Status,Priority,Document State,Location,Email,Phone,Skills,Documents,Submitted Date");

            foreach (var a in items)
            {
                var cleanId = EscapeCsv(a.Id);
                var cleanName = EscapeCsv(a.Applicant);
                var cleanStatus = EscapeCsv(a.Status);
                var cleanPriority = EscapeCsv(a.Priority);
                var cleanDocState = EscapeCsv(a.DocumentState);
                var cleanLocation = EscapeCsv(a.Location);
                var cleanEmail = EscapeCsv(a.Email);
                var cleanPhone = EscapeCsv(a.Phone);
                var cleanSkills = EscapeCsv(string.Join(";", a.Skills));
                var cleanDocs = EscapeCsv(string.Join(";", a.Documents));
                var cleanDate = a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC");

                sb.AppendLine($"\"{cleanId}\",\"{cleanName}\",\"{cleanStatus}\",\"{cleanPriority}\",\"{cleanDocState}\",\"{cleanLocation}\",\"{cleanEmail}\",\"{cleanPhone}\",\"{cleanSkills}\",\"{cleanDocs}\",\"{cleanDate}\"");
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
        if (diff.TotalHours < 24)
        {
            var h = (int)diff.TotalHours;
            return h == 1 ? "1 hour ago" : $"{h} hours ago";
        }
        if (diff.TotalDays < 2) return "Yesterday";
        if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
        return dateTime.ToString("MMM dd, yyyy");
    }
}
