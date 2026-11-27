using CareLink.Application.DTOs.Chatbot;

namespace CareLink.Application.Interfaces;

public interface IChatbotService
{
    Task<ChatbotMessageResponseDto> SendMessageAsync(
        string userId,
        string userRole,
        ChatbotMessageRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChatbotConversationSummaryDto>> GetUserConversationsAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<ChatbotConversationDetailDto?> GetConversationByIdAsync(
        string userId,
        string conversationId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteConversationAsync(
        string userId,
        string conversationId,
        CancellationToken cancellationToken = default);
}
