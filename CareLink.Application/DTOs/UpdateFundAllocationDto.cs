using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class UpdateFundAllocationDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(200)]
    public string Source { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Purpose { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Recipient { get; set; } = string.Empty;

    [Required]
    public DateTime AllocationDate { get; set; }

    [MaxLength(50)]
    public string Approval { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Disbursement { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;
}
