namespace CareLink.Core.Models;

public class ShelterQueryParameters
{
    public string? Search { get; set; }
    public string? District { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? Occupancy { get; set; }
    public int? MinCapacity { get; set; }
    public int? MaxCapacity { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
