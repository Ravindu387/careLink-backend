using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateShelterDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = "Central Ward";
    public string District { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    [Required]
    [Range(1, 50000, ErrorMessage = "Capacity must be at least 1.")]
    public int Capacity { get; set; }

    public int Occupied { get; set; } = 0;
    public List<string>? Services { get; set; }
    public string? Manager { get; set; }
    public string? ContactPerson { get => Manager; set => Manager = value; }
    public string? ContactPhone { get; set; }
    public string? Status { get; set; } = "Active";
    public string? Notes { get; set; }
}
