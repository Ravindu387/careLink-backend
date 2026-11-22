using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateDonationDto
{
    [Required]
    [MaxLength(200)]
    public string Donor { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = "Monetary"; // Monetary | Material

    [Required]
    [MaxLength(100)]
    public string Amount { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Campaign { get; set; } = string.Empty;

    public string? Received { get; set; } // Can be "22 Jun" or ISO date "2025-06-22"
    public string? Verification { get; set; } = "Pending";
    public string? Allocation { get; set; } = "Unallocated";
    public string? Notes { get; set; }
}
