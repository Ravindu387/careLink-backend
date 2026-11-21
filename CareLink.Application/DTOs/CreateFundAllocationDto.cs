using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateFundAllocationDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(200)]
    public string Source { get; set; } = "Flood Response 2025";

    [Required]
    [MaxLength(300)]
    public string Purpose { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Recipient { get; set; } = string.Empty;

    [Required]
    public DateTime AllocationDate { get; set; }

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;
}
