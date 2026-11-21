namespace CareLink.Application.DTOs;

public class CreateShelterDto
{
    public string Name { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Manager { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
}
