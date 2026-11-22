namespace CareLink.Application.DTOs;

public class BedDto
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = "Available";
    public string? Resident { get; set; }
}
