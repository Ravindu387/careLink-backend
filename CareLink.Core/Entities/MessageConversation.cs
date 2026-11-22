namespace CareLink.Core.Entities;

public class MessageConversation
{
    public string Id { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string CitizenName { get; set; } = "Samira Khan";
    public string CitizenId { get; set; } = string.Empty;
    public string? RequestId { get; set; }
    public string Status { get; set; } = "Active";
    public int UnreadCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
    public List<ChatMessage> Messages { get; set; } = new();
}
