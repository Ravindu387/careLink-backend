using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs.Chatbot;

public class ChatbotMessageRequestDto
{
    [Required(ErrorMessage = "Message is required.")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 2000 characters.")]
    public string Message { get; set; } = string.Empty;

    public string? ConversationId { get; set; }
}

public class ChatbotMessageResponseDto
{
    public string ConversationId { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Role { get; set; } = "Citizen";
    public List<string> SuggestedActions { get; set; } = new();
}

public class ChatbotConversationSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int MessageCount { get; set; }
    public string LastMessagePreview { get; set; } = string.Empty;
}

public class ChatbotConversationDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ChatbotMessageDto> Messages { get; set; } = new();
}

public class ChatbotMessageDto
{
    public string Id { get; set; } = string.Empty;
    public string Role { get; set; } = "user"; // "user" or "model"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
