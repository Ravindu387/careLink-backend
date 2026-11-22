namespace CareLink.Core.Entities;

public class Warehouse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Capacity { get; set; }
    public double Utilisation { get; set; }
    public int StockLines { get; set; }
    public int Staff { get; set; }
    public string Status { get; set; } = "Operational";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
