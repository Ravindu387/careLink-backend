using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class UpdateDonationDto
{
    [Required]
    [MaxLength(200)]
    public string Donor { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = "Monetary";

    [Required]
    [MaxLength(100)]
    public string Amount { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Campaign { get; set; } = string.Empty;

    public string? Received { get; set; }
    public string? Verification { get; set; }
    public string? Allocation { get; set; }
    public string? Notes { get; set; }
}

public class UpdateDonationStatusDto
{
    public string? Verification { get; set; }
    public string? Allocation { get; set; }
}
