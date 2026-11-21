namespace CareLink.Core.Models;

public class MaintenanceRequestQueryParameters
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public string? Technician { get; set; }
    public string? Shelter { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
