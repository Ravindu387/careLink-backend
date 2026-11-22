namespace CareLink.Core.Models;

public class NotificationQueryParameters
{
    public string? Search { get; set; }
    public string? Type { get; set; }
    public string? Category { get => Type; set => Type = value; }
    public bool? Unread { get; set; }
    public bool? UnreadOnly { get => Unread; set => Unread = value; }
    public bool? UrgentOnly { get; set; }
    public string? Role { get; set; } = "ShelterAdmin";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
