using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class UpdateExpenditureStatusDto
{
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}
