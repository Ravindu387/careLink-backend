namespace CareLink.Application.DTOs;

public class SafetyCheckDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "green";
    public string IconKey { get; set; } = "Check";
}
