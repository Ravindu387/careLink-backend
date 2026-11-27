using CareLink.Core.Entities;

namespace CareLink.Application.Interfaces;

public interface IChatbotRepository
{
    Task<ChatbotConversation?> GetConversationAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatbotConversation>> GetUserConversationsAsync(string userId, CancellationToken cancellationToken = default);
    Task SaveConversationAsync(ChatbotConversation conversation, CancellationToken cancellationToken = default);
    Task<bool> DeleteConversationAsync(string id, string userId, CancellationToken cancellationToken = default);
}
