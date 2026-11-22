using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class BulkFundAllocationActionDto
{
    [Required]
    public List<string> Ids { get; set; } = new();

    /// <summary>Approve | Reject | Disburse</summary>
    [Required]
    public string Action { get; set; } = string.Empty;
}
