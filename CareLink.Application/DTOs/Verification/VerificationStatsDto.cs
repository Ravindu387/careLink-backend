namespace CareLink.Application.DTOs.Verification;

public class VerificationStatsDto
{
    public int TotalPending { get; set; }
    public int HighPriority { get; set; }
    public int ApprovedToday { get; set; }
    public int AdditionalInfo { get; set; }
    public int OldestPendingHours { get; set; }
    public string OldestPendingText { get; set; } = string.Empty;
    public string SlaTarget { get; set; } = "24 hours";
    public string SlaStatus { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
}
