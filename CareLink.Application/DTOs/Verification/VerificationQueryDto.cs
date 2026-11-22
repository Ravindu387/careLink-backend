namespace CareLink.Application.DTOs.Verification;

public class VerificationQueryDto
{
    public string? Search { get; set; }
    public string? Type { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public string? DateFilter { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
