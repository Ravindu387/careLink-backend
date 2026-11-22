using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class CaseReportsService : ICaseReportsService
{
    private static readonly List<ReportItem> DefaultReportLibrary =
    [
        new()
        {
            Id = "REP-01",
            Title = "Case Intake Report",
            Description = "Analyse incoming cases, request volumes and categories across operational sectors.",
            IconKey = "Inbox",
            Color = "bg-sky-50 text-sky-700"
        },
        new()
        {
            Id = "REP-02",
            Title = "Verification Report",
            Description = "Review verification rates, evidence outcomes, and document authenticity ratios.",
            IconKey = "ShieldCheck",
            Color = "bg-sky-50 text-sky-700"
        },
        new()
        {
            Id = "REP-03",
            Title = "Priority & SLA Report",
            Description = "Analyse priority levels, overdue cases and response turnaround against SLA thresholds.",
            IconKey = "AlertTriangle",
            Color = "bg-sky-50 text-sky-700"
        },
        new()
        {
            Id = "REP-04",
            Title = "Duplicate Review Report",
            Description = "Review automated duplicate similarity detection accuracy and human verification decisions.",
            IconKey = "Copy",
            Color = "bg-sky-50 text-sky-700"
        },
        new()
        {
            Id = "REP-05",
            Title = "Escalation Report",
            Description = "Track escalated cases requiring coordinator intervention and high-risk case resolution.",
            IconKey = "ArrowUpToLine",
            Color = "bg-sky-50 text-sky-700"
        },
        new()
        {
            Id = "REP-06",
            Title = "AI Decision Audit",
            Description = "Review AI recommendations, confidence scores, and caseworker override justifications.",
            IconKey = "Sparkles",
            Color = "bg-sky-50 text-sky-700"
        },
        new()
        {
            Id = "REP-07",
            Title = "Case Manager Performance",
            Description = "Measure caseload distribution, active claim turnaround, and officer resolution efficiency.",
            IconKey = "Users",
            Color = "bg-sky-50 text-sky-700"
        },
        new()
        {
            Id = "REP-08",
            Title = "Operational Area Summary",
            Description = "Compare geographic operational demand, shelter capacity stress, and logistics supply bottlenecks.",
            IconKey = "ChartLine",
            Color = "bg-sky-50 text-sky-700"
        }
    ];

    public Task<ReportsAnalyticsResponse> GetReportsAnalyticsAsync(ReportsFilterDto filters)
    {
        var area = filters.OperationalArea?.Trim() ?? "All areas";

        AnalyticsKpi kpi;
        List<CategoryBreakdown> categories;
        List<DailyResolutionPoint> resolutionTrend;

        if (area.Equals("North Ward", StringComparison.OrdinalIgnoreCase))
        {
            kpi = new AnalyticsKpi
            {
                Received = 645,
                Resolved = 498,
                ResolutionRate = "77.2%",
                MedianResponse = "1h 42m",
                AiOverrides = "7.2%",
                AiOverridesDetails = "46 of 645 reviews",
                ReceivedTrend = "+5% today",
                ResponseTrend = "-22 min this week"
            };

            categories =
            [
                new() { Name = "Food & Water", Value = 162 },
                new() { Name = "Shelter", Value = 145 },
                new() { Name = "Medical", Value = 110 },
                new() { Name = "Transport", Value = 85 },
                new() { Name = "Accessibility", Value = 45 },
                new() { Name = "Other", Value = 98 }
            ];

            resolutionTrend =
            [
                new() { Day = "18 Jun", Received = 45, Resolved = 30 },
                new() { Day = "19 Jun", Received = 55, Resolved = 40 },
                new() { Day = "20 Jun", Received = 60, Resolved = 50 },
                new() { Day = "21 Jun", Received = 80, Resolved = 65 },
                new() { Day = "22 Jun", Received = 75, Resolved = 68 },
                new() { Day = "23 Jun", Received = 95, Resolved = 78 },
                new() { Day = "24 Jun", Received = 110, Resolved = 92 }
            ];
        }
        else if (area.Equals("South Ward", StringComparison.OrdinalIgnoreCase))
        {
            kpi = new AnalyticsKpi
            {
                Received = 523,
                Resolved = 416,
                ResolutionRate = "79.5%",
                MedianResponse = "2h 05m",
                AiOverrides = "9.1%",
                AiOverridesDetails = "48 of 523 reviews",
                ReceivedTrend = "+7% today",
                ResponseTrend = "-15 min this week"
            };

            categories =
            [
                new() { Name = "Food & Water", Value = 135 },
                new() { Name = "Shelter", Value = 115 },
                new() { Name = "Medical", Value = 88 },
                new() { Name = "Transport", Value = 64 },
                new() { Name = "Accessibility", Value = 36 },
                new() { Name = "Other", Value = 85 }
            ];

            resolutionTrend =
            [
                new() { Day = "18 Jun", Received = 35, Resolved = 25 },
                new() { Day = "19 Jun", Received = 42, Resolved = 32 },
                new() { Day = "20 Jun", Received = 48, Resolved = 38 },
                new() { Day = "21 Jun", Received = 62, Resolved = 50 },
                new() { Day = "22 Jun", Received = 58, Resolved = 52 },
                new() { Day = "23 Jun", Received = 78, Resolved = 62 },
                new() { Day = "24 Jun", Received = 88, Resolved = 74 }
            ];
        }
        else if (area.Equals("Eastbank", StringComparison.OrdinalIgnoreCase))
        {
            kpi = new AnalyticsKpi
            {
                Received = 480,
                Resolved = 370,
                ResolutionRate = "77.0%",
                MedianResponse = "2h 30m",
                AiOverrides = "8.1%",
                AiOverridesDetails = "39 of 480 reviews",
                ReceivedTrend = "+9% today",
                ResponseTrend = "-10 min this week"
            };

            categories =
            [
                new() { Name = "Food & Water", Value = 120 },
                new() { Name = "Shelter", Value = 85 },
                new() { Name = "Medical", Value = 75 },
                new() { Name = "Transport", Value = 60 },
                new() { Name = "Accessibility", Value = 30 },
                new() { Name = "Other", Value = 110 }
            ];

            resolutionTrend =
            [
                new() { Day = "18 Jun", Received = 25, Resolved = 15 },
                new() { Day = "19 Jun", Received = 35, Resolved = 22 },
                new() { Day = "20 Jun", Received = 38, Resolved = 30 },
                new() { Day = "21 Jun", Received = 50, Resolved = 40 },
                new() { Day = "22 Jun", Received = 52, Resolved = 42 },
                new() { Day = "23 Jun", Received = 68, Resolved = 52 },
                new() { Day = "24 Jun", Received = 75, Resolved = 60 }
            ];
        }
        else if (area.Equals("West District", StringComparison.OrdinalIgnoreCase))
        {
            kpi = new AnalyticsKpi
            {
                Received = 299,
                Resolved = 218,
                ResolutionRate = "72.9%",
                MedianResponse = "2h 55m",
                AiOverrides = "9.5%",
                AiOverridesDetails = "28 of 299 reviews",
                ReceivedTrend = "+6% today",
                ResponseTrend = "-8 min this week"
            };

            categories =
            [
                new() { Name = "Food & Water", Value = 69 },
                new() { Name = "Shelter", Value = 49 },
                new() { Name = "Medical", Value = 45 },
                new() { Name = "Transport", Value = 37 },
                new() { Name = "Accessibility", Value = 17 },
                new() { Name = "Other", Value = 82 }
            ];

            resolutionTrend =
            [
                new() { Day = "18 Jun", Received = 15, Resolved = 5 },
                new() { Day = "19 Jun", Received = 18, Resolved = 6 },
                new() { Day = "20 Jun", Received = 19, Resolved = 17 },
                new() { Day = "21 Jun", Received = 28, Resolved = 25 },
                new() { Day = "22 Jun", Received = 25, Resolved = 28 },
                new() { Day = "23 Jun", Received = 34, Resolved = 33 },
                new() { Day = "24 Jun", Received = 42, Resolved = 44 }
            ];
        }
        else
        {
            // All areas
            kpi = new AnalyticsKpi
            {
                Received = 1947,
                Resolved = 1502,
                ResolutionRate = "77.1%",
                MedianResponse = "2h 18m",
                AiOverrides = "8.4%",
                AiOverridesDetails = "126 of 1,498 reviews",
                ReceivedTrend = "+8% today",
                ResponseTrend = "-18 min this week"
            };

            categories =
            [
                new() { Name = "Food & Water", Value = 486 },
                new() { Name = "Shelter", Value = 394 },
                new() { Name = "Medical", Value = 318 },
                new() { Name = "Transport", Value = 246 },
                new() { Name = "Accessibility", Value = 128 },
                new() { Name = "Other", Value = 375 }
            ];

            resolutionTrend =
            [
                new() { Day = "18 Jun", Received = 120, Resolved = 75 },
                new() { Day = "19 Jun", Received = 150, Resolved = 100 },
                new() { Day = "20 Jun", Received = 165, Resolved = 135 },
                new() { Day = "21 Jun", Received = 220, Resolved = 180 },
                new() { Day = "22 Jun", Received = 210, Resolved = 190 },
                new() { Day = "23 Jun", Received = 275, Resolved = 225 },
                new() { Day = "24 Jun", Received = 315, Resolved = 270 }
            ];
        }

        var response = new ReportsAnalyticsResponse
        {
            Filters = filters,
            Analytics = kpi,
            Categories = categories,
            ResolutionTrend = resolutionTrend,
            ReportLibrary = DefaultReportLibrary,
            GeneratedAt = DateTime.UtcNow
        };

        return Task.FromResult(response);
    }

    public Task<List<ReportItem>> GetReportLibraryAsync()
    {
        return Task.FromResult(DefaultReportLibrary);
    }

    public async Task<byte[]> ExportReportsCsvAsync(ReportsFilterDto filters)
    {
        var data = await GetReportsAnalyticsAsync(filters);
        var sb = new StringBuilder();

        sb.AppendLine("CareLink - Case Reports & Analytics");
        sb.AppendLine($"Date Range,\"{data.Filters.DateRange}\"");
        sb.AppendLine($"Crisis Event,\"{data.Filters.CrisisEvent}\"");
        sb.AppendLine($"Operational Area,\"{data.Filters.OperationalArea}\"");
        sb.AppendLine($"Generated At,\"{data.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC\"");
        sb.AppendLine();

        sb.AppendLine("Metric,Value,Trend/Details");
        sb.AppendLine($"Cases Received,{data.Analytics.Received},\"{data.Analytics.ReceivedTrend}\"");
        sb.AppendLine($"Cases Resolved,{data.Analytics.Resolved},\"{data.Analytics.ResolutionRate}\"");
        sb.AppendLine($"Median Response Time,\"{data.Analytics.MedianResponse}\",\"{data.Analytics.ResponseTrend}\"");
        sb.AppendLine($"AI Decision Overrides,\"{data.Analytics.AiOverrides}\",\"{data.Analytics.AiOverridesDetails}\"");
        sb.AppendLine();

        sb.AppendLine("Category,Cases Count");
        foreach (var cat in data.Categories)
        {
            sb.AppendLine($"\"{cat.Name}\",{cat.Value}");
        }
        sb.AppendLine();

        sb.AppendLine("Day,Cases Received,Cases Resolved");
        foreach (var r in data.ResolutionTrend)
        {
            sb.AppendLine($"\"{r.Day}\",{r.Received},{r.Resolved}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
