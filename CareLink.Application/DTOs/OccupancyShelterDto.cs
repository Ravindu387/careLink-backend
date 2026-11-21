namespace CareLink.Application.DTOs;

public class OccupancyShelterDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int Occupied { get; set; }
    public int Reserved { get; set; }
    public int Available { get; set; }
    public double OccupancyRate { get; set; }
    public string Status { get; set; } = "Available"; // Available, Warning, Critical
}
