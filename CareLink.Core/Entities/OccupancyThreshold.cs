namespace CareLink.Core.Entities;

public class OccupancyThreshold
{
    public int Id { get; set; } = 1;
    public double Warning { get; set; } = 80;
    public double Critical { get; set; } = 95;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
