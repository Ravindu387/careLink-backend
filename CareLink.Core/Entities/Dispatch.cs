namespace CareLink.Core.Entities;

public class Dispatch
{
    public string Id { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Vehicle { get; set; } = string.Empty;
    public string Driver { get; set; } = string.Empty;
    public string Departed { get; set; } = string.Empty;
    public string Eta { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? CurrentLocation { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
