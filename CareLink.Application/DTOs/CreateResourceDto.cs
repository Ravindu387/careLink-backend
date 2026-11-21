using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateResourceDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = "Food & Water";

    [Range(0, 1000000)]
    public int Available { get; set; }

    [Range(1, 1000000)]
    public int Minimum { get; set; }

    [Required]
    public string Unit { get; set; } = "Units";

    public string? ExpiryDate { get; set; }

    [Required]
    public string Shelter { get; set; } = string.Empty;
}
