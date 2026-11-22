namespace CareLink.Core.Models;

public class FundingDashboardData
{
    public FundingFinancialStats FinancialStats { get; set; } = new();
    public List<FundingWeeklyChartItem> WeeklyChart { get; set; } = new();
    public decimal WeeklyReceivedTotal { get; set; }
    public string WeeklyReceivedDisplay { get; set; } = string.Empty;
    public List<FundingCampaignPerformanceItem> CampaignPerformance { get; set; } = new();
    public List<FundingRecentDonationItem> RecentDonations { get; set; } = new();
    public List<FundingAttentionItem> AttentionItems { get; set; } = new();
}

public class FundingFinancialStats
{
    public decimal MonetaryDonations { get; set; }
    public string MonetaryDonationsDisplay { get; set; } = string.Empty;
    public decimal MonetaryThisWeek { get; set; }
    public string MonetaryThisWeekDisplay { get; set; } = string.Empty;

    public decimal MaterialContributions { get; set; }
    public string MaterialContributionsDisplay { get; set; } = string.Empty;
    public string MaterialNote { get; set; } = "Estimated verified value";

    public decimal AllocatedFunds { get; set; }
    public string AllocatedFundsDisplay { get; set; } = string.Empty;
    public double AllocatedPercentage { get; set; }
    public string AllocatedNote { get; set; } = string.Empty;

    public decimal RecordedExpenditure { get; set; }
    public string RecordedExpenditureDisplay { get; set; } = string.Empty;
    public double ExpenditurePercentage { get; set; }
    public string ExpenditureNote { get; set; } = string.Empty;

    public int ActiveCampaignsCount { get; set; }
    public int CampaignsAboveTargetCount { get; set; }
    public string ActiveCampaignsNote { get; set; } = string.Empty;

    public int PendingVerificationCount { get; set; }
    public decimal PendingVerificationAmount { get; set; }
    public string PendingVerificationDisplay { get; set; } = string.Empty;
    public string PendingVerificationNote { get; set; } = string.Empty;

    public decimal AvailableBalance { get; set; }
    public string AvailableBalanceDisplay { get; set; } = string.Empty;
    public string AvailableBalanceNote { get; set; } = "Unallocated verified funds";

    public int FlaggedRecordsCount { get; set; }
    public int HighPriorityFlagsCount { get; set; }
    public string FlaggedRecordsNote { get; set; } = string.Empty;
}

public class FundingWeeklyChartItem
{
    public string Day { get; set; } = string.Empty; // e.g. "Mon", "Tue", ... "Today"
    public string Date { get; set; } = string.Empty;
    public decimal ReceivedAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public double ReceivedPercentage { get; set; } // scaled for chart height (0-100)
    public double AllocatedPercentage { get; set; } // scaled for chart height (0-100)
}

public class FundingCampaignPerformanceItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Raised { get; set; }
    public decimal Target { get; set; }
    public string DisplayAmount { get; set; } = string.Empty; // e.g. "$1.84M of $2.5M"
    public double Percentage { get; set; }
}

public class FundingRecentDonationItem
{
    public string Reference { get; set; } = string.Empty;
    public string Donor { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string Campaign { get; set; } = string.Empty;
    public string Received { get; set; } = string.Empty;
    public string Verification { get; set; } = string.Empty;
    public string Allocation { get; set; } = string.Empty;
}

public class FundingAttentionItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "info"; // critical | warning | info
    public string? Link { get; set; }
}
