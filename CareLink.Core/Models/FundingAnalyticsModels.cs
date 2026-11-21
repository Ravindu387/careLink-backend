namespace CareLink.Core.Models;

public class FundingAnalyticsQueryParameters
{
    public string? DateRange { get; set; } // "01-30 June 2025", "Last 7 days", "Last 30 days", "Last 90 days", "All time"
    public string? Campaign { get; set; }  // "All campaigns" or specific name
    public string? Currency { get; set; } = "USD"; // "USD", "LKR", "EUR", "GBP"
}

public class FundingAnalyticsReport
{
    public FundingAnalyticsMetrics Metrics { get; set; } = new();
    public FundingAnalyticsTrend Trend { get; set; } = new();
    public List<FundingContributionSource> Sources { get; set; } = new();
    public decimal CombinedSourceValue { get; set; }
    public string CombinedSourceDisplay { get; set; } = string.Empty;
    public List<string> AvailableCampaigns { get; set; } = new();
}

public class FundingAnalyticsMetrics
{
    public decimal FundsReceived { get; set; }
    public string FundsReceivedDisplay { get; set; } = string.Empty;
    public string FundsReceivedGrowth { get; set; } = "+18.4% this month";

    public double AllocationRate { get; set; }
    public decimal AllocatedAmount { get; set; }
    public string AllocatedSubtitle { get; set; } = string.Empty;

    public double ExpenditureRate { get; set; }
    public decimal ExpendedAmount { get; set; }
    public string ExpenditureSubtitle { get; set; } = string.Empty;

    public double VerificationRate { get; set; }
    public int PendingRecordsCount { get; set; }
    public string VerificationSubtitle { get; set; } = string.Empty;
}

public class FundingAnalyticsTrend
{
    public string Growth { get; set; } = "+18.4%";
    public string Currency { get; set; } = "USD";
    public List<FundingTrendPoint> Points { get; set; } = new();
    public string SvgLineD { get; set; } = string.Empty;
    public string SvgAreaD { get; set; } = string.Empty;
}

public class FundingTrendPoint
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CoordinateX { get; set; }
    public int CoordinateY { get; set; }
}

public class FundingContributionSource
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string DisplayAmount { get; set; } = string.Empty;
    public double Percentage { get; set; }
}
