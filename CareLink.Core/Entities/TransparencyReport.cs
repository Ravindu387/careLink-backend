namespace CareLink.Core.Entities;

public class TransparencyReport
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Draft"; // Published, Draft, Under Review
    public string? Description { get; set; }
    public string Period { get; set; } = "18–25 June 2025";
    public decimal VerifiedContributions { get; set; } = 4820000m;
    public decimal AllocatedFunds { get; set; } = 3910000m;
    public decimal Expenditure { get; set; } = 2740000m;
    public decimal AvailableBalance { get; set; } = 910000m;
    public string? Author { get; set; } = "Maria Kone";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
