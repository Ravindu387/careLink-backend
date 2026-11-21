namespace CareLink.Core.Models;

public class ResidentQueryParameters
{
    public string? Search { get; set; }
    public string? ShelterId { get; set; }
    public string? Status { get; set; }
    public string? AssistanceFilter { get; set; }
    public string? Gender { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
