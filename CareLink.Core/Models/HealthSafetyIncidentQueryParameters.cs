namespace CareLink.Core.Models;

public class HealthSafetyIncidentQueryParameters
{
    public string? Search { get; set; }
    public string? Type { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
    public string? DateRange { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
