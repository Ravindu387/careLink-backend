using System.Text;
using CareLink.Backend.Models;

namespace CareLink.Backend.Services;

public class ShelterReportsService : IShelterReportsService
{
    private static readonly Dictionary<string, ShelterMetricsDto> ShelterData = new(StringComparer.OrdinalIgnoreCase)
    {
        ["All shelters"] = new ShelterMetricsDto
        {
            Occupancy = 66.2,
            Admissions = 342,
            Approved = 84,
            Resources = 12480,
            Pending = 14,
            Rejected = 7,
            Departed = 158,
            ApprovalCount = 287,
            OccupancyTrend = "+4.8% in period",
            AdmissionsSubtitle = "158 departures",
            ResourcesSubtitle = "Across 8 categories"
        },
        ["Riverton Central"] = new ShelterMetricsDto
        {
            Occupancy = 92.0,
            Admissions = 128,
            Approved = 88,
            Resources = 4200,
            Pending = 5,
            Rejected = 2,
            Departed = 42,
            ApprovalCount = 113,
            OccupancyTrend = "+6.2% in period",
            AdmissionsSubtitle = "42 departures",
            ResourcesSubtitle = "Across 8 categories"
        },
        ["North Community"] = new ShelterMetricsDto
        {
            Occupancy = 57.0,
            Admissions = 84,
            Approved = 82,
            Resources = 3100,
            Pending = 4,
            Rejected = 2,
            Departed = 38,
            ApprovalCount = 69,
            OccupancyTrend = "+3.1% in period",
            AdmissionsSubtitle = "38 departures",
            ResourcesSubtitle = "Across 6 categories"
        },
        ["St. Mary Civic"] = new ShelterMetricsDto
        {
            Occupancy = 100.0,
            Admissions = 65,
            Approved = 76,
            Resources = 2200,
            Pending = 3,
            Rejected = 2,
            Departed = 32,
            ApprovalCount = 49,
            OccupancyTrend = "+8.5% in period",
            AdmissionsSubtitle = "32 departures",
            ResourcesSubtitle = "Across 5 categories"
        },
        ["Lakeside Sports"] = new ShelterMetricsDto
        {
            Occupancy = 49.0,
            Admissions = 65,
            Approved = 90,
            Resources = 2980,
            Pending = 2,
            Rejected = 1,
            Departed = 46,
            ApprovalCount = 59,
            OccupancyTrend = "-1.4% in period",
            AdmissionsSubtitle = "46 departures",
            ResourcesSubtitle = "Across 7 categories"
        }
    };

    private static readonly List<ShelterReportItem> ReportLibrary =
    [
        new ShelterReportItem
        {
            Id = 1,
            Title = "Shelter Occupancy Report",
            Description = "Capacity, occupancy rates, and available bed allocations across shelters.",
            Category = "Shelter",
            IconName = "FileText",
            Format = "CSV",
            LastGenerated = "Today, 14:10"
        },
        new ShelterReportItem
        {
            Id = 2,
            Title = "Resident Registration Report",
            Description = "Admissions, departures, special needs, and resident demographic profiles.",
            Category = "Residents",
            IconName = "Users",
            Format = "CSV",
            LastGenerated = "Today, 13:45"
        },
        new ShelterReportItem
        {
            Id = 3,
            Title = "Bed Availability Report",
            Description = "Room and bed status, turnover efficiency, and real-time vacancies.",
            Category = "Shelter",
            IconName = "BedDouble",
            Format = "CSV",
            LastGenerated = "Yesterday"
        },
        new ShelterReportItem
        {
            Id = 4,
            Title = "Resource Inventory Report",
            Description = "Stock levels, replenishment alerts, distribution, and expiry schedules.",
            Category = "Resources",
            IconName = "Package",
            Format = "CSV",
            LastGenerated = "Today, 10:15"
        },
        new ShelterReportItem
        {
            Id = 5,
            Title = "Accommodation Request Report",
            Description = "Intake outcomes, assignment times, intake approvals, and reject ratios.",
            Category = "Requests",
            IconName = "ClipboardList",
            Format = "CSV",
            LastGenerated = "2 days ago"
        },
        new ShelterReportItem
        {
            Id = 6,
            Title = "Staff Activity Report",
            Description = "Staff shifts, duty attendance, task rotations, and responsibilities.",
            Category = "Staff",
            IconName = "UserRound",
            Format = "CSV",
            LastGenerated = "Yesterday"
        },
        new ShelterReportItem
        {
            Id = 7,
            Title = "Health & Safety Report",
            Description = "Field incidents, medical aid reports, inspections, and hazard mitigation.",
            Category = "Health",
            IconName = "HeartPulse",
            Format = "CSV",
            LastGenerated = "Today, 08:30"
        },
        new ShelterReportItem
        {
            Id = 8,
            Title = "Shelter Performance Overview",
            Description = "Cross-shelter operational metrics, SLA benchmarks, and resource utilization.",
            Category = "Performance",
            IconName = "ChartNoAxesColumn",
            Format = "CSV",
            LastGenerated = "Today, 11:00"
        }
    ];

    private static readonly List<string> SheltersList =
    [
        "All shelters",
        "Riverton Central",
        "North Community",
        "St. Mary Civic",
        "Lakeside Sports"
    ];

    private static readonly List<string> DistrictsList =
    [
        "All districts",
        "Central District",
        "North District",
        "Eastbank",
        "West District"
    ];

    private static readonly List<string> DateRangesList =
    [
        "18–25 June 2025",
        "Last 7 days",
        "Last 30 days",
        "This month",
        "Last month"
    ];

    public Task<ShelterReportsOverviewDto> GetOverviewAsync(ShelterReportsFilterDto filter)
    {
        var metrics = CalculateMetrics(filter);
        var trend = GenerateTrend(filter, metrics.Occupancy);

        var overview = new ShelterReportsOverviewDto
        {
            Filters = filter,
            Metrics = metrics,
            TrendData = trend,
            Reports = ReportLibrary,
            Shelters = SheltersList,
            Districts = DistrictsList,
            DateRanges = DateRangesList,
            GeneratedAt = DateTime.UtcNow
        };

        return Task.FromResult(overview);
    }

    public Task<ShelterMetricsDto> GetMetricsAsync(ShelterReportsFilterDto filter)
    {
        var metrics = CalculateMetrics(filter);
        return Task.FromResult(metrics);
    }

    public Task<List<ShelterTrendDataPoint>> GetTrendDataAsync(ShelterReportsFilterDto filter)
    {
        var metrics = CalculateMetrics(filter);
        var trend = GenerateTrend(filter, metrics.Occupancy);
        return Task.FromResult(trend);
    }

    public Task<List<ShelterReportItem>> GetReportLibraryAsync()
    {
        return Task.FromResult(ReportLibrary);
    }

    public Task<ShelterReportItem?> GetReportByIdAsync(int id)
    {
        var item = ReportLibrary.FirstOrDefault(r => r.Id == id);
        return Task.FromResult(item);
    }

    public Task<byte[]> ExportOverviewCsvAsync(ShelterReportsFilterDto filter)
    {
        var metrics = CalculateMetrics(filter);
        var trend = GenerateTrend(filter, metrics.Occupancy);

        var sb = new StringBuilder();
        sb.AppendLine("CareLink - Shelter Reports & Analytics Export");
        sb.AppendLine($"Generated At,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"Date Range,\"{filter.DateRange}\"");
        sb.AppendLine($"Shelter,\"{filter.Shelter}\"");
        sb.AppendLine($"District,\"{filter.District}\"");
        sb.AppendLine();
        sb.AppendLine("KEY OPERATIONAL METRICS");
        sb.AppendLine("Metric,Value,Context");
        sb.AppendLine($"Average Occupancy,{metrics.Occupancy}%,{metrics.OccupancyTrend}");
        sb.AppendLine($"New Admissions,{metrics.Admissions},{metrics.AdmissionsSubtitle}");
        sb.AppendLine($"Requests Approved,{metrics.Approved}%,{metrics.ApprovalCount} approved");
        sb.AppendLine($"Resources Distributed,{metrics.Resources},{metrics.ResourcesSubtitle}");
        sb.AppendLine($"Pending Requests,{metrics.Pending},Under verification");
        sb.AppendLine($"Rejected Requests,{metrics.Rejected},Capacity or eligibility limit");
        sb.AppendLine();
        sb.AppendLine("DAILY OCCUPANCY TREND");
        sb.AppendLine("Day,Occupancy (%)");
        foreach (var point in trend)
        {
            sb.AppendLine($"\"{point.Day}\",{point.Value}%");
        }
        sb.AppendLine();
        sb.AppendLine("AVAILABLE REPORT MODULES");
        sb.AppendLine("Report Title,Category,Description");
        foreach (var report in ReportLibrary)
        {
            sb.AppendLine($"\"{report.Title}\",\"{report.Category}\",\"{report.Description}\"");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return Task.FromResult(bytes);
    }

    public Task<byte[]> ExportReportItemCsvAsync(string reportTitle, ShelterReportsFilterDto filter)
    {
        var metrics = CalculateMetrics(filter);
        var matched = ReportLibrary.FirstOrDefault(r =>
            string.Equals(r.Title, reportTitle, StringComparison.OrdinalIgnoreCase))
            ?? ReportLibrary[0];

        var sb = new StringBuilder();
        sb.AppendLine($"CareLink - {matched.Title}");
        sb.AppendLine($"Category,\"{matched.Category}\"");
        sb.AppendLine($"Scope - Shelter,\"{filter.Shelter}\"");
        sb.AppendLine($"Scope - District,\"{filter.District}\"");
        sb.AppendLine($"Date Range,\"{filter.DateRange}\"");
        sb.AppendLine($"Export Timestamp,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        sb.AppendLine("OPERATIONAL BREAKDOWN");
        sb.AppendLine("Metric,Value,Status");
        sb.AppendLine($"Average Occupancy,{metrics.Occupancy}%,Target: <85%");
        sb.AppendLine($"Admissions Count,{metrics.Admissions},Normal intake");
        sb.AppendLine($"Approved Intake,{metrics.ApprovalCount} ({metrics.Approved}%),Compliant");
        sb.AppendLine($"Departures Recorded,{metrics.Departed},Processed");
        sb.AppendLine($"Pending Queue,{metrics.Pending},In Review");
        sb.AppendLine($"Inventory Dispatched,{metrics.Resources} units,Delivered");
        sb.AppendLine();
        sb.AppendLine("AUDIT NOTE");
        sb.AppendLine($"\"This {matched.Title} was generated automatically by CareLink Shelter Operations.\"");

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return Task.FromResult(bytes);
    }

    private static ShelterMetricsDto CalculateMetrics(ShelterReportsFilterDto filter)
    {
        if (!ShelterData.TryGetValue(filter.Shelter, out var baseMetrics))
        {
            baseMetrics = ShelterData["All shelters"];
        }

        double districtMultiplier = 1.0;
        if (!string.IsNullOrWhiteSpace(filter.District) && !filter.District.Equals("All districts", StringComparison.OrdinalIgnoreCase))
        {
            districtMultiplier = filter.District switch
            {
                "Central District" => 1.15,
                "North District" => 0.85,
                "Eastbank" => 0.70,
                "West District" => 0.90,
                _ => 1.0
            };
        }

        int admissions = Math.Max(10, (int)Math.Round(baseMetrics.Admissions * districtMultiplier));
        int approvedPercent = baseMetrics.Approved;
        int approvalCount = (int)Math.Round(admissions * approvedPercent / 100.0);
        int departed = Math.Max(5, (int)Math.Round(baseMetrics.Departed * districtMultiplier));
        int resources = Math.Max(500, (int)Math.Round(baseMetrics.Resources * districtMultiplier));
        int pending = Math.Max(1, (int)Math.Round(baseMetrics.Pending * districtMultiplier));
        int rejected = Math.Max(0, (int)Math.Round(baseMetrics.Rejected * districtMultiplier));

        double occupancy = Math.Round(baseMetrics.Occupancy * (districtMultiplier > 1 ? 1.05 : 0.95), 1);
        if (occupancy > 100.0) occupancy = 100.0;
        if (occupancy < 20.0) occupancy = 20.0;

        return new ShelterMetricsDto
        {
            Occupancy = occupancy,
            Admissions = admissions,
            Approved = approvedPercent,
            Resources = resources,
            Pending = pending,
            Rejected = rejected,
            Departed = departed,
            ApprovalCount = approvalCount,
            OccupancyTrend = baseMetrics.OccupancyTrend,
            AdmissionsSubtitle = $"{departed} departures",
            ResourcesSubtitle = baseMetrics.ResourcesSubtitle
        };
    }

    private static List<ShelterTrendDataPoint> GenerateTrend(ShelterReportsFilterDto filter, double currentOccupancy)
    {
        double factor = currentOccupancy / 66.2;
        var baseTrend = new[]
        {
            ("18 Jun", 48),
            ("19 Jun", 55),
            ("20 Jun", 54),
            ("21 Jun", 65),
            ("22 Jun", 64),
            ("23 Jun", 77),
            ("24 Jun", 83)
        };

        return baseTrend.Select(t => new ShelterTrendDataPoint
        {
            Day = t.Item1,
            Value = Math.Clamp((int)Math.Round(t.Item2 * factor), 15, 100)
        }).ToList();
    }
}
