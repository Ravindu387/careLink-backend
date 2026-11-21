namespace CareLink.Application.DTOs.Notifications;

public class NotificationQueryDto
{
    public string? Search { get; set; }
    public string? Type { get; set; } // "All", "Emergency", "Assistance", "Volunteer", "Resource", "System"
    public string? Priority { get; set; } // "All priorities", "Critical", "High", "Normal"
    public bool? UnreadOnly { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
