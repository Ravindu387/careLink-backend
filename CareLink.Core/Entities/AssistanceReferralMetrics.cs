namespace CareLink.Core.Entities;

public class AssistanceReferralMetrics
{
    public int OpenReferrals { get; set; }
    public int AwaitingAction { get; set; }
    public int CriticalPriority { get; set; }
    public int AcceptedToday { get; set; }
    public int PeopleSupported { get; set; }
    public int AllCount { get; set; }
    public int NeedsActionCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ArchivedCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
