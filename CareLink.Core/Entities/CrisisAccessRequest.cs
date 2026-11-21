namespace CareLink.Core.Entities;

public class CrisisAccessRequest
{
    public string Id { get; set; } = string.Empty;
    public string CrisisId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string PartnerName { get; set; } = "Global Relief Network";
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;
}
