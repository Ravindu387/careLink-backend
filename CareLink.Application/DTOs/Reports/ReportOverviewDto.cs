namespace CareLink.Application.DTOs.Reports;

public class ReportOverviewDto
{
    public List<StatMetricDto> Stats { get; set; } = new();
    public List<TrendPointDto> AdoptionTrend { get; set; } = new();
    public string AdoptionGrowthBadge { get; set; } = "+8.4%";
    public int TotalRegisteredUsers { get; set; }
    public List<RoleDistributionItemDto> UsersByRole { get; set; } = new();
    public List<ReportCatalogItemDto> Reports { get; set; } = new();
    public ReportFilterDto AppliedFilters { get; set; } = new();
}
