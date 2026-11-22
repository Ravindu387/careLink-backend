using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateFlaggedTransactionDto
{
    [Required]
    [MaxLength(50)]
    public string Record { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(200)]
    public string Party { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Severity { get; set; } = "Medium";

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Investigator { get; set; } = "Not assigned";

    [MaxLength(50)]
    public string RecordType { get; set; } = string.Empty;
}
