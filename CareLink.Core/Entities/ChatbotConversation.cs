namespace CareLink.Core.Entities;

public class ChatbotConversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = "New Conversation";
    public string UserRole { get; set; } = "Citizen";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<ChatbotMessage> Messages { get; set; } = new();
}
