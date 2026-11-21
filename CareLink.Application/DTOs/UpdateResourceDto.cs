namespace CareLink.Application.DTOs;

public class UpdateResourceDto
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public int? Available { get; set; }
    public int? Minimum { get; set; }
    public string? Unit { get; set; }
    public string? ExpiryDate { get; set; }
    public string? Shelter { get; set; }
}
