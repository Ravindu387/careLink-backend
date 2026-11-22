using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class RecordDistributionDto
{
    [Required]
    public string ResourceId { get; set; } = string.Empty;

    [Range(1, 1000000)]
    public int Quantity { get; set; }

    [Required]
    public string Shelter { get; set; } = string.Empty;

    [Required]
    public string Recipient { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
