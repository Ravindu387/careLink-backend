namespace CareLink.Application.DTOs;

public class AssistanceRequestQueryParameters
{
    public string? Search { get; set; }
    public string? Tab { get; set; }
    public string? CrisisEvent { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public string? Organisation { get; set; }
    public bool? VerifiedOnly { get; set; }
    public bool? UnassignedOnly { get; set; }
}
