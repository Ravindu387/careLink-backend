namespace CareLink.Core.Models;

public class StaffQueryParameters
{
    public string? Search { get; set; }
    public string? Shelter { get; set; }
    public string? Designation { get; set; }
    public string? Shift { get; set; }
    public string? Availability { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
