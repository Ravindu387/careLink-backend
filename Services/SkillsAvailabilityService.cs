using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class SkillsAvailabilityService : ISkillsAvailabilityService
{
    private readonly object _lock = new();
    private readonly List<SkillCoverageItemDto> _skills = [];
    private readonly List<AvailabilityForecastDto> _forecast = [];
    private readonly List<AvailabilityRequestDto> _requests = [];
    private int _requestCounter = 1;

    public SkillsAvailabilityService()
    {
        InitializeData();
    }

    private void InitializeData()
    {
        _skills.AddRange([
            new SkillCoverageItemDto
            {
                Name = "Logistics & Distribution",
                Verified = 486,
                Available = 184,
                Percentage = 74,
                Category = "Operations"
            },
            new SkillCoverageItemDto
            {
                Name = "First Aid & Medical",
                Verified = 294,
                Available = 96,
                Percentage = 68,
                Category = "Medical"
            },
            new SkillCoverageItemDto
            {
                Name = "Driving & Transport",
                Verified = 318,
                Available = 128,
                Percentage = 82,
                Category = "Logistics"
            },
            new SkillCoverageItemDto
            {
                Name = "Shelter Support",
                Verified = 402,
                Available = 142,
                Percentage = 77,
                Category = "Shelter"
            },
            new SkillCoverageItemDto
            {
                Name = "Translation",
                Verified = 128,
                Available = 52,
                Percentage = 63,
                Category = "Communications"
            },
            new SkillCoverageItemDto
            {
                Name = "Search & Rescue",
                Verified = 94,
                Available = 40,
                Percentage = 91,
                Category = "Emergency"
            },
            new SkillCoverageItemDto
            {
                Name = "Childcare",
                Verified = 176,
                Available = 64,
                Percentage = 58,
                Category = "Support"
            },
            new SkillCoverageItemDto
            {
                Name = "Administration",
                Verified = 332,
                Available = 116,
                Percentage = 70,
                Category = "Management"
            }
        ]);

        _forecast.AddRange([
            new AvailabilityForecastDto { Day = "Mon 23", Volunteers = 520 },
            new AvailabilityForecastDto { Day = "Tue 24", Volunteers = 551 },
            new AvailabilityForecastDto { Day = "Wed 25", Volunteers = 582 },
            new AvailabilityForecastDto { Day = "Thu 26", Volunteers = 613 },
            new AvailabilityForecastDto { Day = "Fri 27", Volunteers = 644 },
            new AvailabilityForecastDto { Day = "Sat 28", Volunteers = 675 },
            new AvailabilityForecastDto { Day = "Sun 29", Volunteers = 706 }
        ]);

        _requests.AddRange([
            new AvailabilityRequestDto
            {
                Id = "REQ-001",
                Skill = "First Aid & Medical",
                Location = "Riverton Central Shelter",
                Date = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
                StartTime = "08:00",
                EndTime = "14:00",
                Volunteers = 5,
                Status = "Approved",
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new AvailabilityRequestDto
            {
                Id = "REQ-002",
                Skill = "Driving & Transport",
                Location = "North Logistics Depot",
                Date = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd"),
                StartTime = "13:00",
                EndTime = "18:00",
                Volunteers = 4,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            }
        ]);
        _requestCounter = 3;
    }

    public Task<SkillsAvailabilityOverviewDto> GetOverviewAsync(string? area = null)
    {
        lock (_lock)
        {
            var overview = new SkillsAvailabilityOverviewDto
            {
                Stats = CalculateStats(),
                Skills = _skills.Select(CloneSkill).ToList(),
                Forecast = _forecast.Select(f => new AvailabilityForecastDto { Day = f.Day, Volunteers = f.Volunteers }).ToList(),
                Warning = new CoverageWarningDto
                {
                    Title = "Thursday coverage gap",
                    Description = "Additional rescue drivers needed from 18:00–22:00.",
                    Severity = "Warning"
                },
                OperationalArea = string.IsNullOrWhiteSpace(area) ? "Greater Riverton" : area,
                GeneratedAt = DateTime.UtcNow
            };

            return Task.FromResult(overview);
        }
    }

    public Task<SkillsAvailabilityStatsDto> GetStatsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(CalculateStats());
        }
    }

    public Task<List<SkillCoverageItemDto>> GetSkillsAsync(string? search = null)
    {
        lock (_lock)
        {
            var query = _skills.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                         s.Category.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
            return Task.FromResult(query.Select(CloneSkill).ToList());
        }
    }

    public Task<SkillCoverageItemDto?> GetSkillByNameAsync(string name)
    {
        lock (_lock)
        {
            var match = _skills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(match != null ? CloneSkill(match) : null);
        }
    }

    public Task<SkillCoverageItemDto> AddSkillAsync(CreateSkillRequest request)
    {
        lock (_lock)
        {
            var existing = _skills.FirstOrDefault(s => s.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                return Task.FromResult(CloneSkill(existing));
            }

            var newSkill = new SkillCoverageItemDto
            {
                Name = request.Name.Trim(),
                Verified = 0,
                Available = 0,
                Percentage = 0,
                Category = string.IsNullOrWhiteSpace(request.Category) ? "Operations" : request.Category.Trim()
            };

            _skills.Add(newSkill);
            return Task.FromResult(CloneSkill(newSkill));
        }
    }

    public Task<bool> RemoveSkillAsync(string name)
    {
        lock (_lock)
        {
            var existing = _skills.FirstOrDefault(s => s.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                return Task.FromResult(false);
            }

            _skills.Remove(existing);
            return Task.FromResult(true);
        }
    }

    public Task<List<AvailabilityForecastDto>> GetForecastAsync(string? area = null)
    {
        lock (_lock)
        {
            return Task.FromResult(_forecast.Select(f => new AvailabilityForecastDto
            {
                Day = f.Day,
                Volunteers = f.Volunteers
            }).ToList());
        }
    }

    public Task<AvailabilityRequestDto> SubmitAvailabilityRequestAsync(AvailabilityRequestDto request)
    {
        lock (_lock)
        {
            request.Id = $"REQ-{_requestCounter++:D3}";
            request.CreatedAt = DateTime.UtcNow;
            request.Status = "Pending";
            _requests.Insert(0, request);
            return Task.FromResult(request);
        }
    }

    public Task<List<AvailabilityRequestDto>> GetAvailabilityRequestsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_requests.ToList());
        }
    }

    public Task<byte[]> ExportSkillsCsvAsync()
    {
        lock (_lock)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CareLink - Volunteer Skills & Availability Coverage");
            sb.AppendLine($"Export Timestamp,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine("Operational Scope,Greater Riverton");
            sb.AppendLine();
            sb.AppendLine("Skill Group,Category,Verified Volunteers,Available Now,Coverage Level (%)");

            foreach (var skill in _skills)
            {
                sb.AppendLine($"\"{EscapeCsv(skill.Name)}\",\"{EscapeCsv(skill.Category)}\",{skill.Verified},{skill.Available},{skill.Percentage}%");
            }

            sb.AppendLine();
            sb.AppendLine("NEXT 7 DAYS FORECAST");
            sb.AppendLine("Day,Expected Available Volunteers");
            foreach (var f in _forecast)
            {
                sb.AppendLine($"\"{EscapeCsv(f.Day)}\",{f.Volunteers}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return Task.FromResult(bytes);
        }
    }

    private SkillsAvailabilityStatsDto CalculateStats()
    {
        int availableTotal = _skills.Sum(s => s.Available);
        int verifiedTotal = _skills.Sum(s => s.Verified);
        double ratio = verifiedTotal > 0 ? (double)availableTotal / verifiedTotal * 100 : 26.8;

        return new SkillsAvailabilityStatsDto
        {
            AvailableToday = availableTotal > 0 ? availableTotal : 642,
            AvailableTodayPercentage = $"{ratio:F1}% of volunteers",
            AvailableThisWeek = 1184,
            AvailableThisWeekSub = "Across all areas",
            CriticalSkillGaps = _skills.Count(s => s.Percentage < 70),
            CriticalSkillGapsSummary = "Rescue, nursing, drivers",
            SchedulesUpdatedRate = "89%",
            SchedulesUpdatedSub = "Within last 7 days"
        };
    }

    private static SkillCoverageItemDto CloneSkill(SkillCoverageItemDto s) => new()
    {
        Name = s.Name,
        Verified = s.Verified,
        Available = s.Available,
        Percentage = s.Percentage,
        Category = s.Category
    };

    private static string EscapeCsv(string val) => (val ?? "").Replace("\"", "\"\"");
}
