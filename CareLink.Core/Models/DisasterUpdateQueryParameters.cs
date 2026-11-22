namespace CareLink.Core.Models;

public class DisasterUpdateQueryParameters
{
    public string? Search { get; set; }
    public string? Type { get; set; }
    public bool? Archived { get; set; }
    public bool? IsDraft { get; set; }
}
