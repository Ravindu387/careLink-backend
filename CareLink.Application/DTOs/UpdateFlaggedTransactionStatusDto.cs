using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class UpdateFlaggedTransactionStatusDto
{
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Investigator { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string ResolutionNotes { get; set; } = string.Empty;
}
