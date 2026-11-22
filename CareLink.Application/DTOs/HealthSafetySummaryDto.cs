namespace CareLink.Application.DTOs;

public class HealthSafetySummaryDto
{
    public int ActionRequired { get; set; }
    public int UnderInvestigation { get; set; }
    public int Resolved { get; set; }
    public int Total { get; set; }
    public IEnumerable<SafetyCheckDto> SafetyChecks { get; set; } = Enumerable.Empty<SafetyCheckDto>();
}
