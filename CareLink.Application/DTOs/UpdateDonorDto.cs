using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class UpdateDonorDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Contact { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Scope { get; set; } = string.Empty;

    public string? Verification { get; set; }
    public decimal? Contributions { get; set; }
    public string? Status { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}

public class UpdateDonorStatusDto
{
    public string? Status { get; set; }
    public string? Verification { get; set; }
}
