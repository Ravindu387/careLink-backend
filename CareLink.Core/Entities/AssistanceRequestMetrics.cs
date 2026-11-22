namespace CareLink.Core.Entities;

public class AssistanceRequestMetrics
{
    public int TotalRequests { get; set; }
    public int Unassigned { get; set; }
    public int Overdue { get; set; }
    public int PotentialDuplicates { get; set; }
    public int UnderReview { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int Escalated { get; set; }
}
