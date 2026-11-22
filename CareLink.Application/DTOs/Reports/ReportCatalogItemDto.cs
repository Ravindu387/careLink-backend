namespace CareLink.Application.DTOs.Reports;

public class ReportCatalogItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;
    public DateTime? LastGenerated { get; set; }
    public int EstimatedRecords { get; set; }
}
