namespace CareLink.Backend.Models;

public class ShelterReportsFilterDto
{
    public string DateRange { get; set; } = "18–25 June 2025";
    public string Shelter { get; set; } = "All shelters";
    public string District { get; set; } = "All districts";
}

public class ShelterMetricsDto
{
    public double Occupancy { get; set; } = 66.2;
    public int Admissions { get; set; } = 342;
    public int Approved { get; set; } = 84;
    public int Resources { get; set; } = 12480;
    public int Pending { get; set; } = 14;
    public int Rejected { get; set; } = 7;
    public int Departed { get; set; } = 158;
    public int ApprovalCount { get; set; } = 287;
    public string OccupancyTrend { get; set; } = "+4.8% in period";
    public string AdmissionsSubtitle { get; set; } = "158 departures";
    public string ResourcesSubtitle { get; set; } = "Across 8 categories";
}

public class ShelterTrendDataPoint
{
    public string Day { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class ShelterReportItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string IconName { get; set; } = "FileText";
    public string Format { get; set; } = "CSV";
    public string LastGenerated { get; set; } = "Today";
}

public class ShelterReportsOverviewDto
{
    public ShelterReportsFilterDto Filters { get; set; } = new();
    public ShelterMetricsDto Metrics { get; set; } = new();
    public List<ShelterTrendDataPoint> TrendData { get; set; } = [];
    public List<ShelterReportItem> Reports { get; set; } = [];
    public List<string> Shelters { get; set; } = [];
    public List<string> Districts { get; set; } = [];
    public List<string> DateRanges { get; set; } = [];
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
