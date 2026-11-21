namespace CareLink.Core.Entities;

public class ChatMessage
{
    public string Id { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Sender { get; set; } = "user"; // "user" or "support"
    public string SenderName { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
