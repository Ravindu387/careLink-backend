namespace CareLink.Application.DTOs.Integrations;

public class IntegrationLogDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string IntegrationId { get; set; } = string.Empty;
    public string Heading { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = "Just now";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Success"; // Success, Warning, Error
}
