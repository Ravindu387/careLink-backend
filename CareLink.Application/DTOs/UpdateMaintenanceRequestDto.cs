namespace CareLink.Application.DTOs;

public class UpdateMaintenanceRequestDto
{
    public string? Issue { get; set; }
    public string? Category { get; set; }
    public string? Shelter { get; set; }
    public string? Room { get; set; }
    public string? Priority { get; set; }
    public string? Technician { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
}
