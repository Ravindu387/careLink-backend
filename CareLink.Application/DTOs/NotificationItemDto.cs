namespace CareLink.Application.DTOs;

public class NotificationItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "System";
    public string Time { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string Icon { get; set; } = "resident";
    public bool Unread { get; set; }
    public string Details { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Category => Type;
    public string Role { get; set; } = "ShelterAdmin";
}
