namespace CareLink.Application.DTOs;

public class MaintenanceRequestMetaDto
{
    public List<string> Categories { get; set; } = new();
    public List<string> Technicians { get; set; } = new();
    public List<string> Shelters { get; set; } = new();
    public List<string> Priorities { get; set; } = new();
    public List<string> Statuses { get; set; } = new();
}
