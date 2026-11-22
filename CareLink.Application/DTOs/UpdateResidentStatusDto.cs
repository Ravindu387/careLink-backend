using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class UpdateResidentStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
