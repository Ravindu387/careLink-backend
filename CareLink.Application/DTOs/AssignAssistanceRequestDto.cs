using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class AssignAssistanceRequestDto
{
    [Required(ErrorMessage = "Assigned team/unit is required.")]
    public string Assigned { get; set; } = string.Empty;

    public string? Organisation { get; set; }
}

public class UpdateAssistanceRequestStatusDto
{
    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = string.Empty;

    public string? Reason { get; set; }
}
