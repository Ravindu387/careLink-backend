namespace CareLink.Application.DTOs;

public class ChatMessageDto
{
    public string Id { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Sender { get; set; } = "user"; // "user" or "support"
    public string SenderName { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ConversationDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Preview { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public int Unread { get; set; }
    public string Category { get; set; } = "General";
    public string CitizenName { get; set; } = string.Empty;
    public string? RequestId { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = new();
}

public class SendMessageRequest
{
    public string Text { get; set; } = string.Empty;
    public string? Sender { get; set; } = "user"; // "user" or "support"
    public string? SenderName { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public bool AutoReply { get; set; } = true; // Auto-generate helpful support response when user messages
}

public class CreateConversationRequest
{
    public string Name { get; set; } = string.Empty; // Support Team name
    public string Subject { get; set; } = string.Empty;
    public string? Category { get; set; } = "General";
    public string? CitizenName { get; set; }
    public string? RequestId { get; set; }
    public string? InitialMessage { get; set; }
}

public class ConversationSummaryDto
{
    public int TotalConversations { get; set; }
    public int TotalUnread { get; set; }
    public int ActiveConversations { get; set; }
}

public class UpdateConversationStatusRequest
{
    public string Status { get; set; } = "Active"; // "Active", "Resolved", "Archived"
}
