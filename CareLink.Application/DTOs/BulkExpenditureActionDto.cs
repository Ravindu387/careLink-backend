using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class BulkExpenditureActionDto
{
    [Required]
    public List<string> Ids { get; set; } = new();

    /// <summary>Approve | Reject | Pay | Hold</summary>
    [Required]
    public string Action { get; set; } = string.Empty;
}
