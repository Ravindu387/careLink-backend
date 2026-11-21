using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class CreateExpenditureDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive.")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Recipient { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Invoice { get; set; } = string.Empty;

    [Required]
    public DateTime ExpenseDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = "Shelter";

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;
}
