using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateDonorDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = "Foundation";

    [Required]
    [MaxLength(150)]
    public string Contact { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Scope { get; set; } = "National";

    public string? Verification { get; set; } = "Pending";
    public decimal Contributions { get; set; } = 0;
    public string? Status { get; set; } = "Active";
    public string? Address { get; set; }
    public string? Notes { get; set; }
}
