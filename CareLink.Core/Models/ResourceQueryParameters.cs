namespace CareLink.Core.Models;

public class ResourceQueryParameters
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public string? ExpiryFilter { get; set; }
    public string? ShelterId { get; set; }
    public int? MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
