namespace CareLink.Application.DTOs;

public class TransparencyReportDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Description { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal VerifiedContributions { get; set; }
    public decimal AllocatedFunds { get; set; }
    public decimal Expenditure { get; set; }
    public decimal AvailableBalance { get; set; }
    public string? Author { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateTransparencyReportDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Period { get; set; }
    public string? Reference { get; set; }
    public string? Author { get; set; }
}

public class UpdateTransparencyReportDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Period { get; set; }
    public string? Status { get; set; }
}

public class TransparencyMetricsDto
{
    public string VerifiedContributionsAmount { get; set; } = "$4.82M";
    public decimal VerifiedContributionsValue { get; set; } = 4820000m;
    public string VerifiedContributionsCount { get; set; } = "1,826 verified records";

    public string AllocatedAmount { get; set; } = "$3.91M";
    public decimal AllocatedValue { get; set; } = 3910000m;
    public string AllocatedPercentage { get; set; } = "81.1% of contributions";

    public string ExpenditureAmount { get; set; } = "$2.74M";
    public decimal ExpenditureValue { get; set; } = 2740000m;
    public string ExpenditurePercentage { get; set; } = "56.8% of contributions";

    public string AvailableBalanceAmount { get; set; } = "$910K";
    public decimal AvailableBalanceValue { get; set; } = 910000m;
    public string AvailableBalancePercentage { get; set; } = "18.9% unallocated";

    public string FeaturedReportTitle { get; set; } = "Flood Response 2025 · Public Funding Summary";
    public string FeaturedReportPeriod { get; set; } = "18–25 June 2025";
    public string FeaturedReportTag { get; set; } = "VERIFIED REPORTING";

    public List<TransparencyAllocationAreaDto> AllocationAreas { get; set; } = new();
    public List<TransparencyCompletenessDto> Completeness { get; set; } = new();
}

public class TransparencyAllocationAreaDto
{
    public string Name { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public int Percentage { get; set; }
}

public class TransparencyCompletenessDto
{
    public string Name { get; set; } = string.Empty;
    public string Percentage { get; set; } = string.Empty;
    public double Value { get; set; }
}
