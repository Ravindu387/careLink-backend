using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class VolunteerReportsService : IVolunteerReportsService
{
    private readonly List<VolunteerReportItem> _reportLibrary = [];
    private readonly List<TrendDataPoint> _baseTrend = [];
    private readonly List<SkillHoursBreakdown> _baseSkills = [];
    private readonly object _lock = new();

    public VolunteerReportsService()
    {
        InitializeData();
    }

    private void InitializeData()
    {
        _reportLibrary.AddRange([
            new VolunteerReportItem
            {
                Id = 1,
                Title = "Volunteer Roster Report",
                Description = "View registered volunteers, availability, declared qualifications and active deployment assignments.",
                IconName = "Users",
                Category = "Volunteer Management",
                LastGenerated = "Today, 14:10",
                Format = "CSV"
            },
            new VolunteerReportItem
            {
                Id = 2,
                Title = "Task Completion Report",
                Description = "Analyze created, assigned and completed response tasks across field wards.",
                IconName = "ListChecks",
                Category = "Task Management",
                LastGenerated = "Today, 12:45",
                Format = "CSV"
            },
            new VolunteerReportItem
            {
                Id = 3,
                Title = "Volunteer Hours Report",
                Description = "Review individual and team volunteer contributed working hours and attendance statistics.",
                IconName = "Clock",
                Category = "Volunteer Performance",
                LastGenerated = "Yesterday",
                Format = "CSV"
            },
            new VolunteerReportItem
            {
                Id = 4,
                Title = "Verification & Compliance",
                Description = "Monitor volunteer verification statuses, safeguarding clearances and qualification compliance.",
                IconName = "ShieldCheck",
                Category = "Compliance",
                LastGenerated = "2 days ago",
                Format = "CSV"
            },
            new VolunteerReportItem
            {
                Id = 5,
                Title = "Training Readiness",
                Description = "Analyze training module completion rates, certification validity and skills readiness.",
                IconName = "Award",
                Category = "Training",
                LastGenerated = "3 days ago",
                Format = "CSV"
            },
            new VolunteerReportItem
            {
                Id = 6,
                Title = "Safety & Incident Report",
                Description = "Review field incidents, emergency alerts and incident-free volunteer operational shifts.",
                IconName = "AlertTriangle",
                Category = "Safety",
                LastGenerated = "Today, 09:15",
                Format = "CSV"
            },
            new VolunteerReportItem
            {
                Id = 7,
                Title = "Performance by Area",
                Description = "Compare volunteer task fulfillment and response times across operational sectors.",
                IconName = "Activity",
                Category = "Performance",
                LastGenerated = "Yesterday",
                Format = "CSV"
            },
            new VolunteerReportItem
            {
                Id = 8,
                Title = "AI Matching Decisions",
                Description = "Review automated volunteer match suitability scores, coordinator approvals and overrides.",
                IconName = "Sparkles",
                Category = "AI Operations",
                LastGenerated = "Today, 11:20",
                Format = "CSV"
            }
        ]);

        _baseTrend.AddRange([
            new TrendDataPoint { Date = "18 Jun", Value = 22, Created = 25, Completed = 22 },
            new TrendDataPoint { Date = "19 Jun", Value = 35, Created = 38, Completed = 35 },
            new TrendDataPoint { Date = "20 Jun", Value = 34, Created = 36, Completed = 34 },
            new TrendDataPoint { Date = "21 Jun", Value = 51, Created = 55, Completed = 51 },
            new TrendDataPoint { Date = "22 Jun", Value = 50, Created = 52, Completed = 50 },
            new TrendDataPoint { Date = "23 Jun", Value = 72, Created = 76, Completed = 72 },
            new TrendDataPoint { Date = "24 Jun", Value = 83, Created = 88, Completed = 83 }
        ]);

        _baseSkills.AddRange([
            new SkillHoursBreakdown { Name = "Logistics", Hours = 3480, Percentage = 24.4 },
            new SkillHoursBreakdown { Name = "Shelter support", Hours = 2860, Percentage = 20.0 },
            new SkillHoursBreakdown { Name = "Medical", Hours = 2240, Percentage = 15.7 },
            new SkillHoursBreakdown { Name = "Transport", Hours = 1980, Percentage = 13.9 },
            new SkillHoursBreakdown { Name = "Translation", Hours = 1120, Percentage = 7.8 },
            new SkillHoursBreakdown { Name = "Rescue", Hours = 980, Percentage = 6.9 }
        ]);
    }

    public Task<VolunteerReportsOverviewDto> GetOverviewAsync(VolunteerReportsQueryDto query)
    {
        lock (_lock)
        {
            var analytics = CalculateAnalytics(query);
            var trend = CalculateTrend(query);
            var skills = CalculateSkills(query);

            var overview = new VolunteerReportsOverviewDto
            {
                Analytics = analytics,
                TrendData = trend,
                SkillData = skills,
                Reports = [.. _reportLibrary],
                OperationalAreas = ["All areas", "North Ward", "Central Ward", "Eastbank", "West District", "South Ward"],
                SkillGroups = ["All skills", "Logistics", "Shelter support", "Medical", "Transport", "Translation", "Rescue"],
                DateRanges = ["18-25 June 2025", "Last 7 days", "Last 30 days", "Last 90 days", "This month", "This year"]
            };

            return Task.FromResult(overview);
        }
    }

    public Task<VolunteerAnalyticsSummaryDto> GetAnalyticsAsync(VolunteerReportsQueryDto query)
    {
        lock (_lock)
        {
            return Task.FromResult(CalculateAnalytics(query));
        }
    }

    public Task<List<TrendDataPoint>> GetTrendDataAsync(VolunteerReportsQueryDto query)
    {
        lock (_lock)
        {
            return Task.FromResult(CalculateTrend(query));
        }
    }

    public Task<List<SkillHoursBreakdown>> GetSkillBreakdownAsync(VolunteerReportsQueryDto query)
    {
        lock (_lock)
        {
            return Task.FromResult(CalculateSkills(query));
        }
    }

    public Task<List<VolunteerReportItem>> GetReportLibraryAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_reportLibrary.ToList());
        }
    }

    public Task<VolunteerReportItem?> GetReportByIdAsync(int id)
    {
        lock (_lock)
        {
            return Task.FromResult(_reportLibrary.FirstOrDefault(r => r.Id == id));
        }
    }

    public Task<byte[]> ExportOverviewCsvAsync(VolunteerReportsQueryDto query)
    {
        lock (_lock)
        {
            var analytics = CalculateAnalytics(query);
            var skills = CalculateSkills(query);

            var sb = new StringBuilder();
            sb.AppendLine("CareLink - Volunteer Reports & Analytics");
            sb.AppendLine($"Date Range,\"{EscapeCsv(query.DateRange ?? "18-25 June 2025")}\"");
            sb.AppendLine($"Operational Area,\"{EscapeCsv(query.OperationalArea ?? "All areas")}\"");
            sb.AppendLine($"Skill Group,\"{EscapeCsv(query.SkillGroup ?? "All skills")}\"");
            sb.AppendLine();

            sb.AppendLine("Metric,Value,Note");
            sb.AppendLine($"Tasks Completed,{analytics.TasksCompleted},\"{analytics.CompletionRate}\"");
            sb.AppendLine($"Volunteer Hours,{analytics.VolunteerHours},\"{analytics.HoursComparison}\"");
            sb.AppendLine($"Acceptance Rate,{analytics.AcceptanceRate:F1}%,\"{analytics.InvitationsCount}\"");
            sb.AppendLine($"Incident-free Shifts,{analytics.IncidentFreeRate:F1}%,\"{analytics.IncidentsSummary}\"");
            sb.AppendLine($"Active Volunteers,{analytics.ActiveVolunteers},Mobilized");
            sb.AppendLine($"Total Shifts Conducted,{analytics.TotalShifts},Recorded");
            sb.AppendLine();

            sb.AppendLine("Skill Group,Hours Contributed,Percentage");
            foreach (var s in skills)
            {
                sb.AppendLine($"\"{EscapeCsv(s.Name)}\",{s.Hours},{s.Percentage:F1}%");
            }

            var utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var result = new byte[utf8Bom.Length + contentBytes.Length];
            Buffer.BlockCopy(utf8Bom, 0, result, 0, utf8Bom.Length);
            Buffer.BlockCopy(contentBytes, 0, result, utf8Bom.Length, contentBytes.Length);

            return Task.FromResult(result);
        }
    }

    public Task<byte[]> ExportReportItemCsvAsync(int id, VolunteerReportsQueryDto query)
    {
        lock (_lock)
        {
            var report = _reportLibrary.FirstOrDefault(r => r.Id == id) ?? new VolunteerReportItem
            {
                Id = id,
                Title = "Operational Report",
                Description = "General volunteer report"
            };

            var sb = new StringBuilder();
            sb.AppendLine($"CareLink Report: {report.Title}");
            sb.AppendLine($"Category: {report.Category}");
            sb.AppendLine($"Filter Date Range: {query.DateRange ?? "18-25 June 2025"}");
            sb.AppendLine($"Filter Area: {query.OperationalArea ?? "All areas"}");
            sb.AppendLine($"Filter Skill Group: {query.SkillGroup ?? "All skills"}");
            sb.AppendLine($"Generated At: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}");
            sb.AppendLine();

            sb.AppendLine("Record ID,Item Name,Area,Status,Assigned Volunteer,Hours,Notes");
            sb.AppendLine($"\"REC-001\",\"{report.Title} Item 1\",\"{query.OperationalArea ?? "Central"}\",\"Completed\",\"Elena Rossi\",18,\"Verified by Coordinator\"");
            sb.AppendLine($"\"REC-002\",\"{report.Title} Item 2\",\"{query.OperationalArea ?? "North Ward"}\",\"Completed\",\"Daniel Okafor\",24,\"Verified by Coordinator\"");
            sb.AppendLine($"\"REC-003\",\"{report.Title} Item 3\",\"{query.OperationalArea ?? "West District"}\",\"Completed\",\"Lin Wei\",12,\"Verified by Coordinator\"");

            var utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var result = new byte[utf8Bom.Length + contentBytes.Length];
            Buffer.BlockCopy(utf8Bom, 0, result, 0, utf8Bom.Length);
            Buffer.BlockCopy(contentBytes, 0, result, utf8Bom.Length, contentBytes.Length);

            return Task.FromResult(result);
        }
    }

    private VolunteerAnalyticsSummaryDto CalculateAnalytics(VolunteerReportsQueryDto query)
    {
        var multiplier = 1.0;
        if (!string.IsNullOrWhiteSpace(query.OperationalArea) && query.OperationalArea != "All areas")
        {
            multiplier *= 0.65;
        }
        if (!string.IsNullOrWhiteSpace(query.SkillGroup) && query.SkillGroup != "All skills")
        {
            multiplier *= 0.45;
        }

        return new VolunteerAnalyticsSummaryDto
        {
            TasksCompleted = (int)Math.Round(1284 * multiplier),
            VolunteerHours = (int)Math.Round(14280 * multiplier),
            AcceptanceRate = 92.0,
            IncidentFreeRate = 98.7,
            ActiveVolunteers = (int)Math.Round(2184 * multiplier),
            TotalShifts = (int)Math.Round(3418 * multiplier),
            CompletionRate = "89.4% completion",
            HoursComparison = "+12% vs previous week",
            InvitationsCount = $"{((int)Math.Round(3418 * multiplier)):N0} invitations",
            IncidentsSummary = "18 minor incidents"
        };
    }

    private List<TrendDataPoint> CalculateTrend(VolunteerReportsQueryDto query)
    {
        var multiplier = 1.0;
        if (!string.IsNullOrWhiteSpace(query.OperationalArea) && query.OperationalArea != "All areas")
        {
            multiplier *= 0.65;
        }
        if (!string.IsNullOrWhiteSpace(query.SkillGroup) && query.SkillGroup != "All skills")
        {
            multiplier *= 0.45;
        }

        return _baseTrend.Select(t => new TrendDataPoint
        {
            Date = t.Date,
            Value = (int)Math.Round(t.Value * multiplier),
            Created = (int)Math.Round(t.Created * multiplier),
            Completed = (int)Math.Round(t.Completed * multiplier)
        }).ToList();
    }

    private List<SkillHoursBreakdown> CalculateSkills(VolunteerReportsQueryDto query)
    {
        var multiplier = 1.0;
        if (!string.IsNullOrWhiteSpace(query.OperationalArea) && query.OperationalArea != "All areas")
        {
            multiplier *= 0.70;
        }

        if (!string.IsNullOrWhiteSpace(query.SkillGroup) && query.SkillGroup != "All skills")
        {
            return _baseSkills
                .Where(s => s.Name.Equals(query.SkillGroup, StringComparison.OrdinalIgnoreCase))
                .Select(s => new SkillHoursBreakdown
                {
                    Name = s.Name,
                    Hours = (int)Math.Round(s.Hours * multiplier),
                    Percentage = 100.0
                }).ToList();
        }

        return _baseSkills.Select(s => new SkillHoursBreakdown
        {
            Name = s.Name,
            Hours = (int)Math.Round(s.Hours * multiplier),
            Percentage = s.Percentage
        }).ToList();
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\"", "\"\"");
    }
}
