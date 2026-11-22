namespace CareLink.Application.DTOs.SecurityEvents;

public class SecurityEventQueryDto
{
    public string? Search { get; set; }
    public string? Severity { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? DateFilter { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
