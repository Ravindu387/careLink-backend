namespace CareLink.Core.Entities;

public class ReportSchedule
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Frequency { get; set; } = "Weekly";
    public string Format { get; set; } = "PDF";
    public string Period { get; set; } = "Last 30 days";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
