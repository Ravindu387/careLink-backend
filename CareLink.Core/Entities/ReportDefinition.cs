namespace CareLink.Core.Entities;

public class ReportDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string IconKey { get; set; } = "file";
    public bool IsActive { get; set; } = true;
}
