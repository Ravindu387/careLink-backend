using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class UpdateFundAllocationStatusDto
{
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}
