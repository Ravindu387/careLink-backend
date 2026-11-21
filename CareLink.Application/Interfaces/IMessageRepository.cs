namespace CareLink.Application.Interfaces;

using CareLink.Core.Entities;

public interface IMessageRepository
{
    Task<IEnumerable<MessageConversation>> GetAllAsync(string? citizenName = null, string? search = null);
    Task<MessageConversation?> GetByIdAsync(string id);
    Task<MessageConversation> CreateConversationAsync(MessageConversation conversation);
    Task<ChatMessage?> AddMessageAsync(string conversationId, ChatMessage message);
    Task<bool> MarkAsReadAsync(string conversationId);
    Task<bool> UpdateStatusAsync(string conversationId, string status);
    Task<bool> DeleteConversationAsync(string id);
    Task<int> GetTotalUnreadCountAsync(string? citizenName = null);
}
